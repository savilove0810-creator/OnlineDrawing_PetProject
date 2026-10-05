using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Rooms.Api.Messaging;
using Rooms.Api.Services;
using RoomsDb = Rooms.Api.Data.AppDbContext;

namespace Drawing.Tests.Infrastructure;

public class RoomsApiFactory : WebApplicationFactory<RoomService>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public RoomsApiFactory()
    {
        _connection.Open();
    }

    public IEventSender EventSender { get; } = Substitute.For<IEventSender>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:Key", JwtCreator.Key);
        builder.UseSetting("ConnectionStrings:DefaultConnection", "Host=unused");
        builder.UseSetting("RabbitMq:ConnectionString", "amqp://unused");
        builder.ConfigureServices(services =>
        {
            services.ReplaceWithSqlite<RoomsDb>(_connection);
            services.RemoveAll<IEventSender>();
            services.AddSingleton(EventSender);
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        host.Services.EnsureDatabase<RoomsDb>();
        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _connection.Dispose();
    }
}
