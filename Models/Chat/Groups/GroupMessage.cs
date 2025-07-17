namespace UnitedGrid.Models.Chat.Groups;

public class GroupMessage : BaseMessage
{
    public required long GroupId { get; set; }

    public Group? Group { get; set; }

    public ICollection<MessageRead> ReadBy { get; set; } = [];

}