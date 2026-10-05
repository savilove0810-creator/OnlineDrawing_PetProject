using System.Net.Http.Headers;

namespace RealtimeBoard.Api.Clients
{
    public class RoomsApiClient : IRoomsApiClient
    {
        public const string HttpClientName = "RoomsApi";

        private readonly IHttpClientFactory _httpClientFactory;

        public RoomsApiClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> RoomExistsAsync(Guid roomId, string? accessToken)
        {
            var client = _httpClientFactory.CreateClient(HttpClientName);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync($"api/rooms/{roomId}");
            return response.IsSuccessStatusCode;
        }
    }
}
