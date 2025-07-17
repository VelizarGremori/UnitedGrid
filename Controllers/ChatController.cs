namespace UnitedGrid.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UnitedGrid.Data;
using UnitedGrid.ViewModels;
using UnitedGrid.Models.Chat;

[Authorize]
public class ChatController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public ChatController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var currentUserId = _userManager.GetUserId(User);
        var chatItems = new List<ChatListItemViewModel>();

        var privateUsers = await _db.Messages
            .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
            .Select(m => m.SenderId == currentUserId ? m.Receiver : m.Sender)
            .Distinct()
            .ToListAsync();

        chatItems.AddRange(privateUsers.Select(u => new ChatListItemViewModel
        {
            Type = ChatType.Private,
            Name = u.GetDisplayName(),
            Id = u.Id
        }));

        var groups = await _db.Groups.ToListAsync();

        chatItems.AddRange(groups.Select(g => new ChatListItemViewModel
        {
            Type = ChatType.Group,
            Name = g.Name,
            Id = g.Id.ToString()
        }));

        return View(chatItems);
    }

    [HttpGet]
    public async Task<IActionResult> SearchUsers(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Json(Array.Empty<string>());

        var currentUserId = _userManager.GetUserId(User);

        var users = await _userManager.Users
            .Where(u => u.UserName.Contains(query) && u.Id != currentUserId)
            .Select(u => new
            {
                id = u.Id,
                name = u.GetDisplayName()
            })
            .Take(10)
            .ToListAsync();

        return Json(users);
    }

    public async Task<IActionResult> Chat(ChatType type, string id)
    {
        var currentUserId = _userManager.GetUserId(User);

        string title;
        List<MessageViewModel> messages;

        if (type == ChatType.Private)
        {
            var receiver = await _userManager.FindByIdAsync(id);
            if (receiver == null) return NotFound();

            title = receiver.GetDisplayName();

            messages = await _db.Messages
                .Include(m => m.Sender)
                .Where(m =>
                    (m.SenderId == currentUserId && m.ReceiverId == id) ||
                    (m.SenderId == id && m.ReceiverId == currentUserId))
                .OrderBy(m => m.SentAt)
                .Select(m => new MessageViewModel
                {
                    Id = m.Id,
                    SenderName = m.Sender.GetDisplayName(),
                    Text = m.Text,
                    SentAt = m.SentAt,
                    IsRead = m.IsRead,
                    Type = ChatType.Private,
                })
                .ToListAsync();

        }
        else
        {
            if (!long.TryParse(id, out var groupId)) return BadRequest();

            var group = await _db.Groups.FindAsync(groupId);
            if (group == null) return NotFound();

            title = group.Name;

            messages = await _db.GroupMessages
                .Include(m => m.Sender)
                .Where(m => m.GroupId == groupId)
                .OrderBy(m => m.SentAt)
                .Select(m => new MessageViewModel
                {
                    Id = m.Id,
                    SenderName = m.Sender.GetDisplayName(),
                    Text = m.Text,
                    SentAt = m.SentAt,
                    IsRead = m.ReadBy.Any(mr => mr.UserId == currentUserId),
                    Type = ChatType.Group,
                })
                .ToListAsync();

        }

        ViewBag.ChatTitle = title;
        ViewBag.ChatType = type;
        ViewBag.ChatId = id;

        return View(messages);
    }
}