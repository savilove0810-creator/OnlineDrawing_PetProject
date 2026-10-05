using Auth.Api.Data.DTOs;
using Auth.Api.Data.Models;
using Auth.Api.Data.Repositories;

namespace Auth.Api.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IJwtService _jwtService;

        public AccountService(IAccountRepository accountRepository, IJwtService jwtService)
        {
            _accountRepository = accountRepository;
            _jwtService = jwtService;
        }

        public async Task RegisterAsync(RegisterDto dto)
        {
            if (await _accountRepository.GetAccountByUsernameAsync(dto.Username) is not null)
                throw new Exception("Пользователь с таким именем уже существует.");

            if (await _accountRepository.GetAccountByEmailAsync(dto.Email) is not null)
                throw new Exception("Пользователь с таким адресом электронной почты уже зарегистрирован.");

            var account = new Account
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            await _accountRepository.AddAccountAsync(account);
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            var account = await _accountRepository.GetAccountByUsernameAsync(username);

            if (account is null || !BCrypt.Net.BCrypt.Verify(password, account.PasswordHash))
                throw new Exception("Неверное имя пользователя или пароль.");

            return _jwtService.GenerateToken(account);
        }
    }
}
