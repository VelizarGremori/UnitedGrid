namespace UnitedGrid.Services;

using Microsoft.AspNetCore.Identity;
using UnitedGrid.Services.Interfaces;

public class CompositeNotificationService
{
    private IEnumerable<INotificationService> _notificationServices;

    public CompositeNotificationService(IEnumerable<INotificationService> notificationServices)
    {
        _notificationServices = notificationServices;
    }

    public async Task NotifyAsync(IdentityUser user, string message)
    {
        foreach (var service in _notificationServices)
        {
            if (await service.CanNotifyAsync(user))
            {
                await service.NotifyAsync(user, message);
            }
        }
    }
}
