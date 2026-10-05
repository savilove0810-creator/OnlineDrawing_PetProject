using System.Text.Json;
using RealtimeBoard.Api.Models;
using StackExchange.Redis;

namespace RealtimeBoard.Api.Presence
{
    public class PresenceStore : IPresenceStore
    {
        private readonly IDatabase _db;

        public PresenceStore(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task AddAsync(Guid roomId, ConnectedUser user)
        {
            var tran = _db.CreateTransaction();
            _ = tran.HashSetAsync($"room:{roomId}", user.ConnectionId, JsonSerializer.Serialize(user));
            _ = tran.StringSetAsync($"connection:{user.ConnectionId}", roomId.ToString());
            _ = tran.StringSetAsync($"user:{user.UserId}:connection", user.ConnectionId);
            await tran.ExecuteAsync();
        }

        public async Task RemoveAsync(Guid roomId, string connectionId)
        {
            var roomKey = $"room:{roomId}";

            var json = await _db.HashGetAsync(roomKey, connectionId);
            var user = json.HasValue ? JsonSerializer.Deserialize<ConnectedUser>((string)json!) : null;

            var tran = _db.CreateTransaction();
            if (user is not null)
            {
                _ = tran.KeyDeleteAsync($"user:{user.UserId}:connection");
            }
            _ = tran.HashDeleteAsync(roomKey, connectionId);
            _ = tran.KeyDeleteAsync($"connection:{connectionId}");
            await tran.ExecuteAsync();
        }

        public async Task<string?> GetExistingConnectionIdAsync(Guid userId)
        {
            var connectionId = await _db.StringGetAsync($"user:{userId}:connection");
            return connectionId.HasValue ? (string)connectionId! : null;
        }

        public async Task<List<ConnectedUser>> GetUsersInRoomAsync(Guid roomId)
        {
            var entries = await _db.HashGetAllAsync($"room:{roomId}");
            return entries
                .Select(e => JsonSerializer.Deserialize<ConnectedUser>((string)e.Value!)!)
                .ToList();
        }

        public async Task<Guid?> GetRoomIdByConnectionAsync(string connectionId)
        {
            var roomId = await _db.StringGetAsync($"connection:{connectionId}");
            return roomId.HasValue ? Guid.Parse((string)roomId!) : null;
        }

        public async Task ClearRoomUsersAsync(Guid roomId)
        {
            var roomKey = $"room:{roomId}";
            var entries = await _db.HashGetAllAsync(roomKey);

            var tran = _db.CreateTransaction();
            foreach (var entry in entries)
            {
                var connectionId = entry.Name.ToString();
                var userJson = entry.Value.ToString();
                if (!string.IsNullOrEmpty(userJson))
                {
                    var user = JsonSerializer.Deserialize<ConnectedUser>(userJson);
                    if (user != null)
                    {
                        _ = tran.KeyDeleteAsync($"user:{user.UserId}:connection");
                    }
                }
                _ = tran.KeyDeleteAsync($"connection:{connectionId}");
            }
            _ = tran.KeyDeleteAsync(roomKey);
            await tran.ExecuteAsync();
        }
    }
}
