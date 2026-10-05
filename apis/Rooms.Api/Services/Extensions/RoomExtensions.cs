using Rooms.Api.Data;
using Rooms.Api.Data.Repositories;
using Rooms.Api.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Rooms.Api.Services.Extensions
{
    public static class RoomExtensions
    {
        public static IServiceCollection AddRoomServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IRoomRepository, RoomRepository>();
            services.AddScoped<IRoomService, RoomService>();

            var rabbitMqConnectionString = configuration["RabbitMq:ConnectionString"]
                ?? throw new InvalidOperationException("RabbitMq:ConnectionString не настроен в конфигурации.");
                
            services.AddSingleton<IEventSender>(_ => RabbitMQEventSender.CreateAsync(rabbitMqConnectionString).GetAwaiter().GetResult());

            return services;
        }
    }
}
