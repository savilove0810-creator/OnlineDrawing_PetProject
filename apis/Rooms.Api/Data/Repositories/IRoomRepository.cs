using Rooms.Api.Data.Models;

namespace Rooms.Api.Data.Repositories
{
    public interface IRoomRepository
    {
        Task AddRoomAsync(Room room);
        Task DeleteRoomAsync(Room room);
        Task<List<Room>> GetRoomsByOwnerAsync(Guid ownerId);
        Task<Room?> GetRoomByroomnameAsync(string roomname);
        Task<Room?> GetRoomByIdAsync(Guid id);
    }
}
