using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Rooms.Api.Data.DTOs;
using Rooms.Api.Data.Models;
using Rooms.Api.Data.Repositories;
using Rooms.Api.Messaging;
using Rooms.Api.Services;

namespace Drawing.Tests.Unit.RoomsApi;

public class RoomServiceTests
{
    private readonly IRoomRepository _repository = Substitute.For<IRoomRepository>();
    private readonly IEventSender _sender = Substitute.For<IEventSender>();
    private readonly RoomService _service;
    private readonly Guid _owner = Guid.NewGuid();

    public RoomServiceTests()
    {
        _service = new RoomService(_repository, _sender);
    }

    [Fact]
    public async Task CreateRoom_SavesRoomWithOwner()
    {
        var room = await _service.CreateRoom(new RoomDto { Name = "board" }, _owner);

        Assert.Equal("board", room.Name);
        Assert.Equal(_owner, room.OwnerId);
        await _repository.Received(1).AddRoomAsync(room);
    }

    [Fact]
    public async Task CreateRoom_DuplicateName_Throws()
    {
        _repository.GetRoomByroomnameAsync("board").Returns(new Room());

        await Assert.ThrowsAsync<Exception>(() => _service.CreateRoom(new RoomDto { Name = "board" }, _owner));
        await _repository.DidNotReceive().AddRoomAsync(Arg.Any<Room>());
    }

    [Fact]
    public async Task DeleteRoom_ByOwner_DeletesAndPublishesEvent()
    {
        var room = new Room { OwnerId = _owner };
        _repository.GetRoomByIdAsync(room.Id).Returns(room);

        await _service.DeleteRoom(room.Id, _owner);

        await _repository.Received(1).DeleteRoomAsync(room);
        await _sender.Received(1).SendEvent("room_deleted", room.Id.ToString());
    }

    [Fact]
    public async Task DeleteRoom_NotFound_Throws()
    {
        _repository.GetRoomByIdAsync(Arg.Any<Guid>()).Returns((Room?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteRoom(Guid.NewGuid(), _owner));
    }

    [Fact]
    public async Task DeleteRoom_NotOwner_Throws()
    {
        var room = new Room { OwnerId = Guid.NewGuid() };
        _repository.GetRoomByIdAsync(room.Id).Returns(room);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.DeleteRoom(room.Id, _owner));
        await _repository.DidNotReceive().DeleteRoomAsync(Arg.Any<Room>());
    }

    [Fact]
    public async Task DeleteRoom_WhenEventSendingFails_StillSucceeds()
    {
        var room = new Room { OwnerId = _owner };
        _repository.GetRoomByIdAsync(room.Id).Returns(room);
        _sender.SendEvent(Arg.Any<string>(), Arg.Any<string>()).ThrowsAsync(new Exception("rabbit down"));

        await _service.DeleteRoom(room.Id, _owner);

        await _repository.Received(1).DeleteRoomAsync(room);
    }

    [Fact]
    public async Task GetRoom_Existing_ReturnsRoom()
    {
        var room = new Room();
        _repository.GetRoomByIdAsync(room.Id).Returns(room);

        Assert.Same(room, await _service.GetRoom(room.Id));
    }

    [Fact]
    public async Task GetRoom_Missing_Throws()
    {
        _repository.GetRoomByIdAsync(Arg.Any<Guid>()).Returns((Room?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetRoom(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetRooms_ReturnsOwnerRooms()
    {
        var rooms = new List<Room> { new() { OwnerId = _owner } };
        _repository.GetRoomsByOwnerAsync(_owner).Returns(rooms);

        Assert.Same(rooms, await _service.GetRooms(_owner));
    }
}
