namespace RealtimeBoard.Api.Services
{
    public interface IRoomSessionService
    {
        Task JoinAsync(Guid roomId, Guid userId, string username, string connectionId, string? accessToken);
        Task LeaveAsync(Guid roomId, string connectionId);
        Task LeaveCurrentRoomAsync(string connectionId);
        Task CloseRoomAsync(Guid roomId, string message, CancellationToken cancellationToken = default);
    }
}
