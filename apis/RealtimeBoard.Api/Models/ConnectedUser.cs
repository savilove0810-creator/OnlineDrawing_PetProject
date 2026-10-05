namespace RealtimeBoard.Api.Models
{
    public class ConnectedUser
    {
        public Guid UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string ConnectionId { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"UserId: {UserId}, Username: {Username}, ConnectionId: {ConnectionId}";
        }
    }
}
