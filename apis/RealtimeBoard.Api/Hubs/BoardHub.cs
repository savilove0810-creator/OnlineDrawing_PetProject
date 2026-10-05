using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RealtimeBoard.Api.Services;

namespace RealtimeBoard.Api.Hubs
{
    [Authorize]
    public class BoardHub : Hub
    {
        public const string Route = "/boardhub";

        private readonly IRoomSessionService _roomSessionService;

        public BoardHub(IRoomSessionService roomSessionService)
        {
            _roomSessionService = roomSessionService;
        }

        public async Task JoinRoom(Guid roomId)
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var usernameClaim = Context.User?.FindFirst(ClaimTypes.Name)?.Value;

            if (userIdClaim is null ||
                usernameClaim is null ||
                !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new HubException("Unauthorized");
            }

            var accessToken = await Context.GetHttpContext()!.GetTokenAsync("access_token");
            await _roomSessionService.JoinAsync(roomId, userId, usernameClaim, Context.ConnectionId, accessToken);
        }

        public Task LeaveRoom(Guid roomId) =>
            _roomSessionService.LeaveAsync(roomId, Context.ConnectionId);

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await _roomSessionService.LeaveCurrentRoomAsync(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task StartStroke(Guid roomId, Guid strokeId, double x, double y, string color, double width)
        {
            await Clients.OthersInGroup(roomId.ToString()).SendAsync("StrokeStarted", strokeId, x, y, color, width);
        }

        public async Task AddPoint(Guid roomId, Guid strokeId, double x, double y)
        {
            await Clients.OthersInGroup(roomId.ToString()).SendAsync("PointAdded", strokeId, x, y);
        }

        public async Task EndStroke(Guid roomId, Guid strokeId)
        {
            await Clients.OthersInGroup(roomId.ToString()).SendAsync("StrokeEnded", strokeId);
        }
    }
}
