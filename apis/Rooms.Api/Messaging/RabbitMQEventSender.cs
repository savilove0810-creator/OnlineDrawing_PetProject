using RabbitMQ.Client;

namespace Rooms.Api.Messaging
{
    public class RabbitMQEventSender : IEventSender 
    {
        private readonly IChannel _channel;

        private RabbitMQEventSender(IChannel channel)
        {
            _channel = channel;
        }

        public static async Task<RabbitMQEventSender> CreateAsync(string connectionString)
        {
            var factory = new ConnectionFactory()
            {
                Uri = new Uri(connectionString)
            };

            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            return new RabbitMQEventSender(channel);
        }

        public async Task SendEvent(string queueName, string message)
        {
            await _channel.QueueDeclareAsync(queue: queueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var body = System.Text.Encoding.UTF8.GetBytes(message);

    

            await _channel.BasicPublishAsync(exchange: "",
                                 routingKey: queueName,
                                 body: body);
        }

    }
}
