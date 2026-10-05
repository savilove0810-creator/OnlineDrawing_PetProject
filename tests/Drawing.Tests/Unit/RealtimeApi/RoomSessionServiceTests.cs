using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using RealtimeBoard.Api.Clients;
using RealtimeBoard.Api.Hubs;
using RealtimeBoard.Api.Models;
using RealtimeBoard.Api.Presence;
using RealtimeBoard.Api.Services;

namespace Drawing.Tests.Unit.RealtimeApi;

public class RoomSessionServiceTests
{
    private readonly IPresenceStore _presence = Substitute.For<IPresenceStore>();
    private readonly IRoomsApiClient _roomsApi = Substitute.For<IRoomsApiClient>();
    private readonly IHubContext<BoardHub> _hub = Substitute.For<IHubContext<BoardHub>>();
    private readonly IHubClients _clients = Substitute.For<IHubClients>();
    private readonly IGroupManager _groups = Substitute.For<IGroupManager>();
    private readonly IClientProxy _groupProxy = Substitute.For<IClientProxy>();
    private readonly ISingleClientProxy _clientProxy = Substitute.For<ISingleClientProxy>();
    private readonly RoomSessionService _service;
    private readonly Guid _roomId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public RoomSessionServiceTests()
    {
        _hub.Clients.Returns(_clients);
        _hub.Groups.Returns(_groups);
        _clients.Group(Arg.Any<string>()).Returns(_groupProxy);
        _clients.Client(Arg.Any<string>()).Returns(_clientProxy);
        _roomsApi.RoomExistsAsync(Arg.Any<Guid>(), Arg.Any<string?>()).Returns(true);
        _presence.GetExistingConnectionIdAsync(Arg.Any<Guid>()).Returns((string?)null);
        _presence.GetRoomIdByConnectionAsync(Arg.Any<string>()).Returns((Guid?)null);
        _presence.GetUsersInRoomAsync(Arg.Any<Guid>()).Returns(new List<ConnectedUser>());
        _service = new RoomSessionService(_presence, _roomsApi, _hub);
    }

    [Fact]
    public async Task JoinAsync_RoomMissing_ThrowsHubException()
    {
        _roomsApi.RoomExistsAsync(_roomId, "token").Returns(false);

        await Assert.ThrowsAsync<HubException>(() => _service.JoinAsync(_roomId, _userId, "alice", "c1", "token"));
        await _presence.DidNotReceive().AddAsync(Arg.Any<Guid>(), Arg.Any<ConnectedUser>());
    }

    [Fact]
    public async Task JoinAsync_AddsToGroupAndPresenceAndNotifies()
    {
        await _service.JoinAsync(_roomId, _userId, "alice", "c1", "token");

        await _groups.Received(1).AddToGroupAsync("c1", _roomId.ToString(), Arg.Any<CancellationToken>());
        await _presence.Received(1).AddAsync(_roomId, Arg.Is<ConnectedUser>(u => u.UserId == _userId && u.ConnectionId == "c1" && u.Username == "alice"));
        await _groupProxy.Received(1).SendCoreAsync("UsersUpdated", Arg.Any<object?[]>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task JoinAsync_SameRoomAgain_IsIdempotent()
    {
        _presence.GetRoomIdByConnectionAsync("c1").Returns(_roomId);

        await _service.JoinAsync(_roomId, _userId, "alice", "c1", "token");

        await _presence.DidNotReceive().AddAsync(Arg.Any<Guid>(), Arg.Any<ConnectedUser>());
    }

    [Fact]
    public async Task JoinAsync_UserHasOtherConnection_ForceDisconnectsOldOne()
    {
        var oldRoom = Guid.NewGuid();
        _presence.GetExistingConnectionIdAsync(_userId).Returns("old");
        _presence.GetRoomIdByConnectionAsync("old").Returns(oldRoom);

        await _service.JoinAsync(_roomId, _userId, "alice", "new", "token");

        await _presence.Received(1).RemoveAsync(oldRoom, "old");
        await _clientProxy.Received(1).SendCoreAsync("ForceDisconnected", Arg.Any<object?[]>(), Arg.Any<CancellationToken>());
        await _presence.Received(1).AddAsync(_roomId, Arg.Is<ConnectedUser>(u => u.ConnectionId == "new"));
    }

    [Fact]
    public async Task JoinAsync_FromAnotherRoom_LeavesPreviousRoom()
    {
        var oldRoom = Guid.NewGuid();
        _presence.GetRoomIdByConnectionAsync("c1").Returns(oldRoom);

        await _service.JoinAsync(_roomId, _userId, "alice", "c1", "token");

        await _presence.Received(1).RemoveAsync(oldRoom, "c1");
        await _presence.Received(1).AddAsync(_roomId, Arg.Any<ConnectedUser>());
    }

    [Fact]
    public async Task LeaveCurrentRoomAsync_NotInRoom_DoesNothing()
    {
        await _service.LeaveCurrentRoomAsync("c1");

        await _presence.DidNotReceive().RemoveAsync(Arg.Any<Guid>(), Arg.Any<string>());
    }

    [Fact]
    public async Task LeaveCurrentRoomAsync_InRoom_RemovesFromRoom()
    {
        _presence.GetRoomIdByConnectionAsync("c1").Returns(_roomId);

        await _service.LeaveCurrentRoomAsync("c1");

        await _presence.Received(1).RemoveAsync(_roomId, "c1");
        await _groups.Received(1).RemoveFromGroupAsync("c1", _roomId.ToString(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CloseRoomAsync_NotifiesGroupAndClearsPresence()
    {
        await _service.CloseRoomAsync(_roomId, "deleted");

        await _groupProxy.Received(1).SendCoreAsync("RoomDeleted", Arg.Any<object?[]>(), Arg.Any<CancellationToken>());
        await _presence.Received(1).ClearRoomUsersAsync(_roomId);
    }
}
