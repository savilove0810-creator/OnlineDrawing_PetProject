using Drawing.Shared.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using RealtimeBoard.Api.Clients;
using RealtimeBoard.Api.Hubs;
using RealtimeBoard.Api.Messaging;
using RealtimeBoard.Api.Presence;
using RealtimeBoard.Api.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var redisConnectionString = builder.Configuration["Redis:ConnectionString"]
    ?? throw new InvalidOperationException("Redis:ConnectionString не настроен в конфигурации.");
var roomsApiBaseUrl = builder.Configuration["RoomsApi:BaseUrl"]
    ?? throw new InvalidOperationException("RoomsApi:BaseUrl не настроен в конфигурации.");

builder.Services.AddOpenApi();
builder.Services.AddSignalR();

builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
builder.Services.AddSingleton<IPresenceStore, PresenceStore>();

builder.Services.AddHttpClient(RoomsApiClient.HttpClientName, client =>
    client.BaseAddress = new Uri(roomsApiBaseUrl.TrimEnd('/') + "/"));
builder.Services.AddSingleton<IRoomsApiClient, RoomsApiClient>();

builder.Services.AddSingleton<IRoomSessionService, RoomSessionService>();
builder.Services.AddHostedService<RoomDeletedConsumer>();

builder.Services.AddJwtAuthentication(builder.Configuration, options =>
{
    options.SaveToken = true;
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments(BoardHub.Route))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<BoardHub>(BoardHub.Route);

app.Run();