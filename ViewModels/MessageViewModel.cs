namespace UnitedGrid.ViewModels;

using UnitedGrid.Models.Chat;

public class MessageViewModel
{
    public long Id { get; set; }

    public required string Text { get; set; }

    public required DateTime SentAt { get; set; }

    public required string SenderName { get; set; }

    public required bool IsRead { get; set; }

    public required ChatType Type { get; set; }
}