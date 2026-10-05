using Rooms.Api.Data.DTOs;
using Rooms.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Rooms.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _roomservice;

        public RoomsController(IRoomService roomservice)
        {
            _roomservice = roomservice;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromBody] RoomDto room)
        {
            return Ok(await _roomservice.CreateRoom(room, GetUserId()));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteRoom(Guid id)
        {
            await _roomservice.DeleteRoom(id, GetUserId());
            return NoContent();
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetRoom(Guid id)
        {
            return Ok(await _roomservice.GetRoom(id));
        }

        [HttpGet]
        public async Task<IActionResult> GetRooms()
        {
            return Ok(await _roomservice.GetRooms(GetUserId()));
        }

        private Guid GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (claim is null || !Guid.TryParse(claim, out var userId))
            {
                throw new UnauthorizedAccessException();
            }
            return userId;
        }
    }
}
