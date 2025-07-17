namespace UnitedGrid.Models.Chat;


using Microsoft.AspNetCore.Identity;

public class Message : BaseMessage
{
    public required string ReceiverId { get; set; }

    public IdentityUser? Receiver { get; set; }

    public bool IsRead { get; set; } = false;
}