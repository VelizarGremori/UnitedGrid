namespace UnitedGrid.Hubs;

using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UnitedGrid.Data;
using UnitedGrid.Models.Chat;
using UnitedGrid.Models.Chat.Groups;
using UnitedGrid.Services.Interfaces;

public class ChatHub : Hub
{
    private readonly ApplicationDbContext _db;
    private readonly IPresenceService _presence;

    public ChatHub(ApplicationDbContext db, IPresenceService presence)
    {
        _db = db;
        _presence = presence;
    }

    public async Task JoinGroup(string groupId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (userId != null)
        {
            await _presence.MarkOnline(userId, Context.ConnectionId);
            await Clients.All.SendAsync("UserOnline", userId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (userId != null)
        {
            await _presence.MarkOffline(userId, Context.ConnectionId);
            await Clients.All.SendAsync("UserOffline", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendPrivateMessage(string receiverId, string message)
    {
        var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(receiverId)) return;

        var chatMessage = new Message
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Text = message
        };

        _db.Messages.Add(chatMessage);
        await _db.SaveChangesAsync();

        var sender = Context.User?.Identity?.Name;

        await Clients.User(senderId).SendAsync("ReceivePrivateMessage", sender, message);
        await Clients.User(receiverId).SendAsync("ReceivePrivateMessage", sender, message);
    }

    public async Task SendGroupMessage(string groupId, string message)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = Context.User?.Identity?.Name;

        if (string.IsNullOrEmpty(userId)) return;

        var groupMessage = new GroupMessage
        {
            SenderId = userId,
            GroupId = long.Parse(groupId),
            Text = message
        };

        _db.GroupMessages.Add(groupMessage);
        await _db.SaveChangesAsync();

        await Clients.Group(groupId).SendAsync("ReceiveGroupMessage", userName, message);
    }

    public async Task MarkPrivateMessageAsRead(int messageId)
    {
        var message = await _db.Messages.FindAsync(messageId);
        if (message?.ReceiverId == Context.UserIdentifier)
        {
            message.IsRead = true;
            await _db.SaveChangesAsync();

            await Clients.User(message.SenderId)
                .SendAsync("PrivateMessageRead", messageId);
        }
    }

    public async Task MarkGroupMessageAsRead(int messageId)
    {
        var userId = Context.UserIdentifier!;
        var already = await _db.MessageReads.FindAsync(messageId, userId);
        if (already == null)
        {
            _db.MessageReads.Add(new MessageRead
            {
                GroupMessageId = messageId,
                UserId = userId
            });
            await _db.SaveChangesAsync();

            var message = await _db.GroupMessages
                .Include(m => m.Sender)
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message != null)
            {
                await Clients.User(message.SenderId)
                    .SendAsync("GroupMessageRead", messageId, userId);
            }
        }
    }
}