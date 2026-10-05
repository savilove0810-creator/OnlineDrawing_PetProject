using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RealtimeBoard.Api.Services;

namespace RealtimeBoard.Api.Messaging
{
    public class RoomDeletedConsumer : BackgroundService
    {
        private const string RoomDeletedQueue = "room_deleted";

        private readonly string _connectionString;
        private readonly IRoomSessionService _roomSessionService;

        public RoomDeletedConsumer(IConfiguration configuration, IRoomSessionService roomSessionService)
        {
            _connectionString = configuration["RabbitMq:ConnectionString"]
                ?? throw new InvalidOperationException("RabbitMq:ConnectionString не настроен в конфигурации.");
            _roomSessionService = roomSessionService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory { Uri = new Uri(_connectionString) };

            await using var connection = await factory.CreateConnectionAsync(stoppingToken);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await channel.QueueDeclareAsync(
                queue: RoomDeletedQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    var roomId = Guid.Parse(Encoding.UTF8.GetString(ea.Body.ToArray()));
                    await _roomSessionService.CloseRoomAsync(roomId, "Комната была удалена владельцем.", stoppingToken);
                }
                catch
                {
                }

                await channel.BasicAckAsync(ea.DeliveryTag, false);
            };

            await channel.BasicConsumeAsync(
                queue: RoomDeletedQueue,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
