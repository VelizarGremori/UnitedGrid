namespace UnitedGrid.Models.Chat.Groups;

public class Group
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public IReadOnlyCollection<GroupMessage> Messages { get; set; } = new List<GroupMessage>();
    public IReadOnlyCollection<GroupUser> Members { get; set; } = new List<GroupUser>();
}