using System.Collections.Concurrent;
using RealtimeBoard.Api.Models;
using RealtimeBoard.Api.Presence;

namespace Drawing.Tests.Infrastructure;

public class FakePresenceStore : IPresenceStore
{
    private readonly ConcurrentDictionary<string, (Guid RoomId, ConnectedUser User)> _connections = new();

    public async Task AddAsync(Guid roomId, ConnectedUser user)
    {
        await Task.CompletedTask;
        _connections[user.ConnectionId] = (roomId, user);
    }

    public async Task RemoveAsync(Guid roomId, string connectionId)
    {
        await Task.CompletedTask;
        _connections.TryRemove(connectionId, out _);
    }

    public async Task<string?> GetExistingConnectionIdAsync(Guid userId)
    {
        await Task.CompletedTask;
        return _connections.Values.FirstOrDefault(c => c.User.UserId == userId).User?.ConnectionId;
    }

    public async Task<List<ConnectedUser>> GetUsersInRoomAsync(Guid roomId)
    {
        await Task.CompletedTask;
        return _connections.Values.Where(c => c.RoomId == roomId).Select(c => c.User).ToList();
    }

    public async Task<Guid?> GetRoomIdByConnectionAsync(string connectionId)
    {
        await Task.CompletedTask;
        return _connections.TryGetValue(connectionId, out var entry) ? entry.RoomId : null;
    }

    public async Task ClearRoomUsersAsync(Guid roomId)
    {
        await Task.CompletedTask;
        foreach (var pair in _connections.Where(c => c.Value.RoomId == roomId).ToList())
            _connections.TryRemove(pair.Key, out _);
    }
}
