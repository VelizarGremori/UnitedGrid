namespace UnitedGrid.Models.Chat.Groups;

using Microsoft.AspNetCore.Identity;

public class GroupUser
{
    public required long GroupId { get; set; }
    public Group? Group { get; set; }

    public required string UserId { get; set; }
    public IdentityUser? User { get; set; }
}