using Auth.Api.Data.Models;

namespace Auth.Api.Data.Repositories
{
    public interface IAccountRepository
    {
        Task AddAccountAsync(Account account);
        Task<Account?> GetAccountByUsernameAsync(string username);
        Task<Account?> GetAccountByEmailAsync(string email);
    }
}
