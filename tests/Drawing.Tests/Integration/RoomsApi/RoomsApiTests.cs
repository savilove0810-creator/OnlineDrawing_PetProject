using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Drawing.Tests.Infrastructure;
using NSubstitute;
using Rooms.Api.Data.Models;

namespace Drawing.Tests.Integration.RoomsApi;

public class RoomsApiTests : IClassFixture<RoomsApiFactory>
{
    private readonly RoomsApiFactory _factory;

    public RoomsApiTests(RoomsApiFactory factory)
    {
        _factory = factory;
    }

    private HttpClient ClientFor(Guid userId)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JwtCreator.Create(userId));
        return client;
    }

    private static string NewName() => "room-" + Guid.NewGuid().ToString("N");

    [Fact]
    public async Task GetRooms_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _factory.CreateClient().GetAsync("/api/rooms");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateRoom_ThenGetById_ReturnsRoom()
    {
        var owner = Guid.NewGuid();
        var client = ClientFor(owner);

        var created = await (await client.PostAsJsonAsync("/api/rooms", new { name = NewName() })).Content.ReadFromJsonAsync<Room>();
        var response = await client.GetAsync($"/api/rooms/{created!.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(owner, created.OwnerId);
        Assert.Equal(created.Id, (await response.Content.ReadFromJsonAsync<Room>())!.Id);
    }

    [Fact]
    public async Task CreateRoom_DuplicateName_ReturnsBadRequest()
    {
        var client = ClientFor(Guid.NewGuid());
        var name = NewName();
        await client.PostAsJsonAsync("/api/rooms", new { name });

        var response = await client.PostAsJsonAsync("/api/rooms", new { name });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetRooms_ReturnsOnlyCurrentUserRooms()
    {
        var mine = ClientFor(Guid.NewGuid());
        var other = ClientFor(Guid.NewGuid());
        await mine.PostAsJsonAsync("/api/rooms", new { name = NewName() });
        await mine.PostAsJsonAsync("/api/rooms", new { name = NewName() });
        await other.PostAsJsonAsync("/api/rooms", new { name = NewName() });

        var rooms = await mine.GetFromJsonAsync<List<Room>>("/api/rooms");

        Assert.Equal(2, rooms!.Count);
    }

    [Fact]
    public async Task GetRoom_Unknown_ReturnsBadRequest()
    {
        var response = await ClientFor(Guid.NewGuid()).GetAsync($"/api/rooms/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteRoom_ByOwner_ReturnsNoContentAndPublishesEvent()
    {
        var client = ClientFor(Guid.NewGuid());
        var created = await (await client.PostAsJsonAsync("/api/rooms", new { name = NewName() })).Content.ReadFromJsonAsync<Room>();

        var response = await client.DeleteAsync($"/api/rooms/{created!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        await _factory.EventSender.Received(1).SendEvent("room_deleted", created.Id.ToString());
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync($"/api/rooms/{created.Id}")).StatusCode);
    }

    [Fact]
    public async Task DeleteRoom_ByNonOwner_ReturnsBadRequestAndKeepsRoom()
    {
        var owner = ClientFor(Guid.NewGuid());
        var stranger = ClientFor(Guid.NewGuid());
        var created = await (await owner.PostAsJsonAsync("/api/rooms", new { name = NewName() })).Content.ReadFromJsonAsync<Room>();

        var response = await stranger.DeleteAsync($"/api/rooms/{created!.Id}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await owner.GetAsync($"/api/rooms/{created.Id}")).StatusCode);
    }
}
