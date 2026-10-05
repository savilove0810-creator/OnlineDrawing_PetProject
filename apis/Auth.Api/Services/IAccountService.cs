using Auth.Api.Data.DTOs;

namespace Auth.Api.Services
{
    public interface IAccountService
    {
        Task RegisterAsync(RegisterDto dto);
        Task<string> LoginAsync(string username, string password);
    }
}
