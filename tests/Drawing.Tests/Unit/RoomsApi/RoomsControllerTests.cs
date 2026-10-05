using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Rooms.Api.Controllers;
using Rooms.Api.Data.DTOs;
using Rooms.Api.Data.Models;
using Rooms.Api.Services;

namespace Drawing.Tests.Unit.RoomsApi;

public class RoomsControllerTests
{
    private readonly IRoomService _service = Substitute.For<IRoomService>();

    private RoomsController Create(params Claim[] claims) => new(_service)
    {
        ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test")) }
        }
    };

    [Fact]
    public async Task GetRooms_WithoutUserClaim_ThrowsUnauthorized()
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Create().GetRooms());
    }

    [Fact]
    public async Task GetRooms_WithInvalidUserClaim_ThrowsUnauthorized()
    {
        var controller = Create(new Claim(ClaimTypes.NameIdentifier, "not-a-guid"));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => controller.GetRooms());
    }

    [Fact]
    public async Task GetRooms_PassesUserIdToService()
    {
        var userId = Guid.NewGuid();
        var rooms = new List<Room>();
        _service.GetRooms(userId).Returns(rooms);

        var result = await Create(new Claim(ClaimTypes.NameIdentifier, userId.ToString())).GetRooms();

        Assert.Same(rooms, Assert.IsType<OkObjectResult>(result).Value);
    }

    [Fact]
    public async Task CreateRoom_UsesCurrentUserAsOwner()
    {
        var userId = Guid.NewGuid();
        var dto = new RoomDto { Name = "board" };

        await Create(new Claim(ClaimTypes.NameIdentifier, userId.ToString())).CreateRoom(dto);

        await _service.Received(1).CreateRoom(dto, userId);
    }

    [Fact]
    public async Task DeleteRoom_ReturnsNoContent()
    {
        var userId = Guid.NewGuid();
        var roomId = Guid.NewGuid();

        var result = await Create(new Claim(ClaimTypes.NameIdentifier, userId.ToString())).DeleteRoom(roomId);

        Assert.IsType<NoContentResult>(result);
        await _service.Received(1).DeleteRoom(roomId, userId);
    }
}
