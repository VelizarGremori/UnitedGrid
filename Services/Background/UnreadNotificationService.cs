namespace UnitedGrid.Services.Background;

using Microsoft.EntityFrameworkCore;
using UnitedGrid.Data;
using UnitedGrid.Services.Interfaces;

public class UnreadNotificationService : BackgroundService
{
    private readonly IServiceProvider _services;

    //TODO Затрется при рестарте и будут повторные уведомления. Либо хранить в сообщениях состояние нотификации, либо сохранять куда-то переменные сервиса(нерекомендуется, но проще)
    private DateTime? _lastNotificationTime;

    public UnreadNotificationService(IServiceProvider services)
    {
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _lastNotificationTime = DateTime.UtcNow;
        while (!stoppingToken.IsCancellationRequested)
        {
            var notificationTime = DateTime.UtcNow;

            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var presence = scope.ServiceProvider.GetRequiredService<IPresenceService>();
            var compositeNotificationService = _services.GetRequiredService<CompositeNotificationService>();

            var threshold = notificationTime.AddMinutes(-5);

            var privateMessages = await db.Messages
                .Where(m => !m.IsRead && m.SentAt <= threshold && m.SentAt > _lastNotificationTime)
                .ToListAsync(stoppingToken);

            var notificationTasks = new List<Task>();

            foreach (var msg in privateMessages)
            {
                if (!await presence.IsOnline(msg.ReceiverId))
                {
                    notificationTasks.Add(compositeNotificationService.NotifyAsync(msg.Receiver, msg.Text));
                }
            }

            var groupMessages = await db.GroupMessages
                .Include(m => m.ReadBy)
                .Where(m => m.SentAt <= threshold && m.SentAt > _lastNotificationTime)
                .ToListAsync(stoppingToken);

            foreach (var msg in groupMessages)
            {
                var groupUsers = await db.GroupUsers
                            .Where(gu => gu.GroupId == msg.GroupId)
                            .Select(gu => gu.User)
                            .ToListAsync();

                var unreadUsers = groupUsers
                    .Where(u => u.Id != msg.SenderId && !msg.ReadBy.Any(r => r.UserId == u.Id));

                foreach (var user in unreadUsers)
                {
                    if (!await presence.IsOnline(user.Id))
                    {
                        notificationTasks.Add(compositeNotificationService.NotifyAsync(user, msg.Text));
                    }
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
