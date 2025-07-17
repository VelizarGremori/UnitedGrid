namespace UnitedGrid.Models.Chat;

using Microsoft.AspNetCore.Identity;

public abstract class BaseMessage
{
    public long Id { get; set; }

    public required string Text { get; set; } 

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public required string SenderId { get; set; }
    public IdentityUser? Sender { get; set; } = null!;
}