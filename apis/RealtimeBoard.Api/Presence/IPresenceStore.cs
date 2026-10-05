using RealtimeBoard.Api.Models;

namespace RealtimeBoard.Api.Presence
{
    public interface IPresenceStore
    {
        Task AddAsync(Guid roomId, ConnectedUser user);
        Task RemoveAsync(Guid roomId, string connectionId);
        Task<string?> GetExistingConnectionIdAsync(Guid userId);
        Task<List<ConnectedUser>> GetUsersInRoomAsync(Guid roomId);
        Task<Guid?> GetRoomIdByConnectionAsync(string connectionId);
        Task ClearRoomUsersAsync(Guid roomId);
    }
}
