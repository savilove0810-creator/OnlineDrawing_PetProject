using Microsoft.AspNetCore.SignalR;
using RealtimeBoard.Api.Clients;
using RealtimeBoard.Api.Hubs;
using RealtimeBoard.Api.Models;
using RealtimeBoard.Api.Presence;

namespace RealtimeBoard.Api.Services
{
    public class RoomSessionService : IRoomSessionService
    {
        private readonly IPresenceStore _presenceStore;
        private readonly IRoomsApiClient _roomsApiClient;
        private readonly IHubContext<BoardHub> _hubContext;

        public RoomSessionService(IPresenceStore presenceStore, IRoomsApiClient roomsApiClient, IHubContext<BoardHub> hubContext)
        {
            _presenceStore = presenceStore;
            _roomsApiClient = roomsApiClient;
            _hubContext = hubContext;
        }

        public async Task JoinAsync(Guid roomId, Guid userId, string username, string connectionId, string? accessToken)
        {
            if (!await _roomsApiClient.RoomExistsAsync(roomId, accessToken))
            {
                throw new HubException("Комната не найдена.");
            }

            var existingConnectionId = await _presenceStore.GetExistingConnectionIdAsync(userId);
            if (existingConnectionId is not null && existingConnectionId != connectionId)
            {
                var oldRoomId = await _presenceStore.GetRoomIdByConnectionAsync(existingConnectionId);
                if (oldRoomId.HasValue)
                {
                    await LeaveAsync(oldRoomId.Value, existingConnectionId);
                }
                await _hubContext.Clients.Client(existingConnectionId).SendAsync("ForceDisconnected", "Вы зашли с другой вкладки.");
            }

            var currentRoomId = await _presenceStore.GetRoomIdByConnectionAsync(connectionId);
            if (currentRoomId == roomId)
            {
                return;
            }
            if (currentRoomId.HasValue)
            {
                await LeaveAsync(currentRoomId.Value, connectionId);
            }

            await _hubContext.Groups.AddToGroupAsync(connectionId, roomId.ToString());
            await _presenceStore.AddAsync(roomId, new ConnectedUser { UserId = userId, Username = username, ConnectionId = connectionId });

            await NotifyUsersAsync(roomId);
        }

        public async Task LeaveAsync(Guid roomId, string connectionId)
        {
            await _hubContext.Groups.RemoveFromGroupAsync(connectionId, roomId.ToString());
            await _presenceStore.RemoveAsync(roomId, connectionId);

            await NotifyUsersAsync(roomId);
        }

        public async Task LeaveCurrentRoomAsync(string connectionId)
        {
            var roomId = await _presenceStore.GetRoomIdByConnectionAsync(connectionId);
            if (roomId.HasValue)
            {
                await LeaveAsync(roomId.Value, connectionId);
            }
        }

        public async Task CloseRoomAsync(Guid roomId, string message, CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients.Group(roomId.ToString()).SendAsync("RoomDeleted", message, cancellationToken);
            await _presenceStore.ClearRoomUsersAsync(roomId);
        }

        private async Task NotifyUsersAsync(Guid roomId)
        {
            var users = await _presenceStore.GetUsersInRoomAsync(roomId);
            await _hubContext.Clients.Group(roomId.ToString()).SendAsync("UsersUpdated", roomId.ToString(), users);
        }
    }
}
