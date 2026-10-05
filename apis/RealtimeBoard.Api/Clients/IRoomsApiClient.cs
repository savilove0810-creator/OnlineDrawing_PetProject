namespace RealtimeBoard.Api.Clients
{
    public interface IRoomsApiClient
    {
        Task<bool> RoomExistsAsync(Guid roomId, string? accessToken);
    }
}
