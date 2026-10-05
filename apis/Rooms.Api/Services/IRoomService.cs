using Rooms.Api.Data.DTOs;
using Rooms.Api.Data.Models;

namespace Rooms.Api.Services
{
    public interface IRoomService
    {
        Task<Room> CreateRoom(RoomDto dto, Guid ownerId);
        Task DeleteRoom(Guid roomId, Guid ownerId);
        Task<Room> GetRoom(Guid roomId);
        Task<List<Room>> GetRooms(Guid ownerId);
    }
}
