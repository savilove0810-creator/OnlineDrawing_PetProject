using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using RealtimeBoard.Api.Clients;
using RealtimeBoard.Api.Presence;
using RealtimeBoard.Api.Services;

namespace Drawing.Tests.Infrastructure;

public class RealtimeApiFactory : WebApplicationFactory<RoomSessionService>
{
    public FakePresenceStore Presence { get; } = new();

    public IRoomsApiClient RoomsApi { get; } = CreateRoomsApi();

    private static IRoomsApiClient CreateRoomsApi()
    {
        var client = Substitute.For<IRoomsApiClient>();
        client.RoomExistsAsync(Arg.Any<Guid>(), Arg.Any<string?>()).Returns(true);
        return client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:Key", JwtCreator.Key);
        builder.UseSetting("Redis:ConnectionString", "localhost:0");
        builder.UseSetting("RoomsApi:BaseUrl", "http://localhost:1");
        builder.UseSetting("RabbitMq:ConnectionString", "amqp://unused");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IHostedService>();
            services.RemoveAll<IPresenceStore>();
            services.RemoveAll<IRoomsApiClient>();
            services.AddSingleton<IPresenceStore>(Presence);
            services.AddSingleton(RoomsApi);
        });
    }
}
