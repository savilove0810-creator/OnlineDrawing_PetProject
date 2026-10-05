using Drawing.Tests.Infrastructure;
using Rooms.Api.Data.Models;
using Rooms.Api.Data.Repositories;
using RoomsDb = Rooms.Api.Data.AppDbContext;

namespace Drawing.Tests.Integration.RoomsApi;

public class RoomRepositoryTests : IDisposable
{
    private readonly SqliteDb<RoomsDb> _db = new();

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task AddRoomAsync_PersistsRoom()
    {
        var room = new Room { Name = "board", OwnerId = Guid.NewGuid() };

        await new RoomRepository(_db.Create()).AddRoomAsync(room);

        var found = await new RoomRepository(_db.Create()).GetRoomByIdAsync(room.Id);
        Assert.Equal("board", found!.Name);
    }

    [Fact]
    public async Task GetRoomByroomnameAsync_FindsByName()
    {
        var repository = new RoomRepository(_db.Create());
        await repository.AddRoomAsync(new Room { Name = "board", OwnerId = Guid.NewGuid() });

        Assert.NotNull(await new RoomRepository(_db.Create()).GetRoomByroomnameAsync("board"));
        Assert.Null(await new RoomRepository(_db.Create()).GetRoomByroomnameAsync("other"));
    }

    [Fact]
    public async Task GetRoomsByOwnerAsync_ReturnsOnlyOwnerRooms()
    {
        var owner = Guid.NewGuid();
        var repository = new RoomRepository(_db.Create());
        await repository.AddRoomAsync(new Room { Name = "a", OwnerId = owner });
        await repository.AddRoomAsync(new Room { Name = "b", OwnerId = owner });
        await repository.AddRoomAsync(new Room { Name = "c", OwnerId = Guid.NewGuid() });

        var rooms = await new RoomRepository(_db.Create()).GetRoomsByOwnerAsync(owner);

        Assert.Equal(2, rooms.Count);
    }

    [Fact]
    public async Task DeleteRoomAsync_RemovesRoom()
    {
        var room = new Room { Name = "board", OwnerId = Guid.NewGuid() };
        var repository = new RoomRepository(_db.Create());
        await repository.AddRoomAsync(room);

        await repository.DeleteRoomAsync(room);

        Assert.Null(await new RoomRepository(_db.Create()).GetRoomByIdAsync(room.Id));
    }
}
