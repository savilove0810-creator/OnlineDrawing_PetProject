using Rooms.Api.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Rooms.Api.Data.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly AppDbContext _context;

        public RoomRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddRoomAsync(Room room)
        {
            await _context.Rooms.AddAsync(room);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRoomAsync(Room room)
        {
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Room>> GetRoomsByOwnerAsync(Guid ownerId)
        {
            return await _context.Rooms.Where(r => r.OwnerId == ownerId).ToListAsync();
        }

        public async Task<Room?> GetRoomByroomnameAsync(string roomname)
        {
            return await _context.Rooms.FirstOrDefaultAsync(r => r.Name == roomname);
        }

        public async Task<Room?> GetRoomByIdAsync(Guid id)
        {
            return await _context.Rooms
                .FirstOrDefaultAsync(r => r.Id == id);
        }


    }
}
