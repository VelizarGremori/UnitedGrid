namespace UnitedGrid.ViewModels;

using UnitedGrid.Models.Chat;

public class ChatListItemViewModel
{
    public ChatType Type { get; set; }
    public required string Name { get; set; }
    public required string Id { get; set; }
}