using Auth.Api.Data.DTOs;
using Auth.Api.Data.Models;
using Auth.Api.Data.Repositories;
using Auth.Api.Services;
using NSubstitute;

namespace Drawing.Tests.Unit.AuthApi;

public class AccountServiceTests
{
    private readonly IAccountRepository _repository = Substitute.For<IAccountRepository>();
    private readonly IJwtService _jwt = Substitute.For<IJwtService>();
    private readonly AccountService _service;

    public AccountServiceTests()
    {
        _service = new AccountService(_repository, _jwt);
    }

    private static RegisterDto Dto() => new() { Username = "alice", Email = "alice@test.com", Password = "secret1" };

    [Fact]
    public async Task RegisterAsync_SavesAccountWithHashedPassword()
    {
        Account? saved = null;
        await _repository.AddAccountAsync(Arg.Do<Account>(a => saved = a));

        await _service.RegisterAsync(Dto());

        Assert.NotNull(saved);
        Assert.Equal("alice", saved!.Username);
        Assert.NotEqual("secret1", saved.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("secret1", saved.PasswordHash));
    }

    [Fact]
    public async Task RegisterAsync_DuplicateUsername_Throws()
    {
        _repository.GetAccountByUsernameAsync("alice").Returns(new Account());

        await Assert.ThrowsAsync<Exception>(() => _service.RegisterAsync(Dto()));
        await _repository.DidNotReceive().AddAccountAsync(Arg.Any<Account>());
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_Throws()
    {
        _repository.GetAccountByEmailAsync("alice@test.com").Returns(new Account());

        await Assert.ThrowsAsync<Exception>(() => _service.RegisterAsync(Dto()));
        await _repository.DidNotReceive().AddAccountAsync(Arg.Any<Account>());
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        var account = new Account { Username = "alice", PasswordHash = BCrypt.Net.BCrypt.HashPassword("secret1") };
        _repository.GetAccountByUsernameAsync("alice").Returns(account);
        _jwt.GenerateToken(account).Returns("token");

        var token = await _service.LoginAsync("alice", "secret1");

        Assert.Equal("token", token);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_Throws()
    {
        var account = new Account { Username = "alice", PasswordHash = BCrypt.Net.BCrypt.HashPassword("secret1") };
        _repository.GetAccountByUsernameAsync("alice").Returns(account);

        await Assert.ThrowsAsync<Exception>(() => _service.LoginAsync("alice", "wrong"));
    }

    [Fact]
    public async Task LoginAsync_UnknownUser_Throws()
    {
        _repository.GetAccountByUsernameAsync("ghost").Returns((Account?)null);

        await Assert.ThrowsAsync<Exception>(() => _service.LoginAsync("ghost", "secret1"));
    }
}
