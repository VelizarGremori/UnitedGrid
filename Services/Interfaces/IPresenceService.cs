namespace UnitedGrid.Services.Interfaces;
public interface IPresenceService
{
    Task MarkOnline(string userId, string connectionId);
    Task MarkOffline(string userId, string connectionId);
    Task<bool> IsOnline(string userId);
    Task<IReadOnlyCollection<string>> GetOnlineUserIds();
}
