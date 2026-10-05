namespace Rooms.Api.Messaging
{
    public interface IEventSender
    {
        Task SendEvent(string queueName, string message);
    }
}
