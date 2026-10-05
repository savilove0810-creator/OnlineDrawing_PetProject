using Rooms.Api.Data.DTOs;
using Rooms.Api.Data.Models;
using Rooms.Api.Data.Repositories;
using Rooms.Api.Messaging;

namespace Rooms.Api.Services
{
    public class RoomService : IRoomService
    {
        private const string RoomDeletedQueue = "room_deleted";

        private readonly IRoomRepository _roomRepository;
        private readonly IEventSender _eventSender;

        public RoomService(IRoomRepository roomRepository, IEventSender eventSender)
        {
            _roomRepository = roomRepository;
            _eventSender = eventSender;
        }

        public async Task<Room> CreateRoom(RoomDto dto, Guid ownerId)
        {
            var existingRoom = await _roomRepository.GetRoomByroomnameAsync(dto.Name);

            if (existingRoom != null)
            {
                throw new Exception("Комната с таким именем уже существует.");
            }

            var room = new Room
            {
                Name = dto.Name,
                OwnerId = ownerId
            };
            await _roomRepository.AddRoomAsync(room);

            return room;
        }

        public async Task DeleteRoom(Guid roomId, Guid ownerId)
        {
            var room = await _roomRepository.GetRoomByIdAsync(roomId);

            if (room == null)
                throw new KeyNotFoundException("Комната не найдена.");

            if (room.OwnerId != ownerId)
                throw new UnauthorizedAccessException("Вы не являетесь владельцем этой комнаты.");

            await _roomRepository.DeleteRoomAsync(room);

            try
            {
                await _eventSender.SendEvent(RoomDeletedQueue, roomId.ToString());
            }
            catch
            {

            }
        }
        public async Task<Room> GetRoom(Guid roomId)
        {
            var room = await _roomRepository.GetRoomByIdAsync(roomId);

            if (room == null)
                throw new KeyNotFoundException("Комната не найдена.");

            return room;
        }

        public async Task<List<Room>> GetRooms(Guid ownerId)
        {
            return await _roomRepository.GetRoomsByOwnerAsync(ownerId);
        }
    }
}
