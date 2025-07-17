namespace UnitedGrid.Models.Chat;

using Microsoft.AspNetCore.Identity;
using UnitedGrid.Models.Chat.Groups;

public class MessageRead
{
    public required long GroupMessageId { get; set; }
    public GroupMessage Message { get; set; } = null!;

    public required string UserId { get; set; }
    public IdentityUser User { get; set; } = null!;
}