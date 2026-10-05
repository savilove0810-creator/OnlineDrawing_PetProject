namespace Rooms.Api.Data.Models
{
    public class Room
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public Guid OwnerId { get; set; }
    }
}
