using Drawing.Tests.Infrastructure;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using NSubstitute;
using RealtimeBoard.Api.Models;

namespace Drawing.Tests.Integration.RealtimeApi;

public class BoardHubTests : IClassFixture<RealtimeApiFactory>, IAsyncDisposable
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    private readonly RealtimeApiFactory _factory;
    private readonly List<HubConnection> _connections = new();

    public BoardHubTests(RealtimeApiFactory factory)
    {
        _factory = factory;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var connection in _connections)
            await connection.DisposeAsync();
    }

    private HubConnection CreateConnection(string? token)
    {
        var connection = new HubConnectionBuilder()
            .WithUrl(new Uri(_factory.Server.BaseAddress, "/boardhub"), options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                options.Transports = HttpTransportType.LongPolling;
                if (token is not null)
                    options.AccessTokenProvider = () => Task.FromResult<string?>(token);
            })
            .Build();
        _connections.Add(connection);
        return connection;
    }

    private async Task<HubConnection> ConnectAsync(string username)
    {
        var connection = CreateConnection(JwtCreator.Create(Guid.NewGuid(), username));
        await connection.StartAsync().WaitAsync(Timeout);
        return connection;
    }

    [Fact]
    public async Task Connect_WithoutToken_IsRejected()
    {
        var connection = CreateConnection(null);

        await Assert.ThrowsAnyAsync<Exception>(() => connection.StartAsync().WaitAsync(Timeout));
    }

    [Fact]
    public async Task JoinRoom_NotifiesRoomWithUsers()
    {
        var roomId = Guid.NewGuid();
        var connection = await ConnectAsync("alice");
        var updated = new TaskCompletionSource<List<ConnectedUser>>();
        connection.On<string, List<ConnectedUser>>("UsersUpdated", (_, users) => updated.TrySetResult(users));

        await connection.InvokeAsync("JoinRoom", roomId).WaitAsync(Timeout);

        var users = await updated.Task.WaitAsync(Timeout);
        Assert.Single(users);
        Assert.Equal("alice", users[0].Username);
    }

    [Fact]
    public async Task JoinRoom_RoomMissing_Fails()
    {
        _factory.RoomsApi.RoomExistsAsync(Arg.Any<Guid>(), Arg.Any<string?>()).Returns(false);
        try
        {
            var connection = await ConnectAsync("alice");

            await Assert.ThrowsAnyAsync<Exception>(() => connection.InvokeAsync("JoinRoom", Guid.NewGuid()).WaitAsync(Timeout));
        }
        finally
        {
            _factory.RoomsApi.RoomExistsAsync(Arg.Any<Guid>(), Arg.Any<string?>()).Returns(true);
        }
    }

    [Fact]
    public async Task StrokeEvents_ReachOtherClientsButNotSender()
    {
        var roomId = Guid.NewGuid();
        var strokeId = Guid.NewGuid();
        var sender = await ConnectAsync("alice");
        var receiver = await ConnectAsync("bob");

        var started = new TaskCompletionSource<Guid>();
        var point = new TaskCompletionSource<double>();
        var ended = new TaskCompletionSource<Guid>();
        var senderReceived = false;
        receiver.On<Guid, double, double, string, double>("StrokeStarted", (id, _, _, _, _) => started.TrySetResult(id));
        receiver.On<Guid, double, double>("PointAdded", (_, x, _) => point.TrySetResult(x));
        receiver.On<Guid>("StrokeEnded", id => ended.TrySetResult(id));
        sender.On<Guid, double, double, string, double>("StrokeStarted", (_, _, _, _, _) => senderReceived = true);

        await sender.InvokeAsync("JoinRoom", roomId).WaitAsync(Timeout);
        await receiver.InvokeAsync("JoinRoom", roomId).WaitAsync(Timeout);
        await sender.InvokeAsync("StartStroke", roomId, strokeId, 1.0, 2.0, "#000", 3.0).WaitAsync(Timeout);
        await sender.InvokeAsync("AddPoint", roomId, strokeId, 5.0, 6.0).WaitAsync(Timeout);
        await sender.InvokeAsync("EndStroke", roomId, strokeId).WaitAsync(Timeout);

        Assert.Equal(strokeId, await started.Task.WaitAsync(Timeout));
        Assert.Equal(5.0, await point.Task.WaitAsync(Timeout));
        Assert.Equal(strokeId, await ended.Task.WaitAsync(Timeout));
        Assert.False(senderReceived);
    }

    [Fact]
    public async Task SecondConnectionOfSameUser_ForceDisconnectsFirst()
    {
        var userId = Guid.NewGuid();
        var token = JwtCreator.Create(userId, "alice");
        var roomId = Guid.NewGuid();
        var first = CreateConnection(token);
        var second = CreateConnection(token);
        var forced = new TaskCompletionSource();
        first.On<string>("ForceDisconnected", _ => forced.TrySetResult());
        await first.StartAsync().WaitAsync(Timeout);
        await second.StartAsync().WaitAsync(Timeout);
        await first.InvokeAsync("JoinRoom", roomId).WaitAsync(Timeout);

        await second.InvokeAsync("JoinRoom", roomId).WaitAsync(Timeout);

        await forced.Task.WaitAsync(Timeout);
        Assert.True(forced.Task.IsCompletedSuccessfully);
    }
}
