namespace UnitedGrid.Services;

using StackExchange.Redis;
using UnitedGrid.Services.Interfaces;
public class PresenceService : IPresenceService
{
    private readonly IConnectionMultiplexer _redis;

    public PresenceService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task MarkOnline(string userId, string connectionId)
    {
        var db = _redis.GetDatabase();

        await db.SetAddAsync($"online:{userId}", connectionId);
        await db.SetAddAsync("onlineUsers", userId);
    }

    public async Task MarkOffline(string userId, string connectionId)
    {
        var db = _redis.GetDatabase();

        await db.SetRemoveAsync($"online:{userId}", connectionId);

        if (await db.SetLengthAsync($"online:{userId}") == 0)
        {
            await db.KeyDeleteAsync($"online:{userId}");
            await db.SetRemoveAsync("onlineUsers", userId);
        }
    }

    public async Task<bool> IsOnline(string userId)
    {
        var db = _redis.GetDatabase();
        return await db.SetLengthAsync($"online:{userId}") > 0;
    }

    public async Task<IReadOnlyCollection<string>> GetOnlineUserIds()
    {
        var db = _redis.GetDatabase();

        var userIds = await db.SetMembersAsync("onlineUsers");

        return [.. userIds.Select(u => u.ToString())];
    }
}