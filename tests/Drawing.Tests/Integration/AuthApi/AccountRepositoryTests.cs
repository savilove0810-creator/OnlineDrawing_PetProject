using Auth.Api.Data.Models;
using Auth.Api.Data.Repositories;
using Drawing.Tests.Infrastructure;
using AuthDb = Auth.Api.Data.AppDbContext;

namespace Drawing.Tests.Integration.AuthApi;

public class AccountRepositoryTests : IDisposable
{
    private readonly SqliteDb<AuthDb> _db = new();

    public void Dispose() => _db.Dispose();

    private static Account NewAccount(string username = "alice", string email = "alice@test.com") =>
        new() { Username = username, Email = email, PasswordHash = "hash" };

    [Fact]
    public async Task AddAccountAsync_PersistsAccount()
    {
        var account = NewAccount();

        await new AccountRepository(_db.Create()).AddAccountAsync(account);

        Assert.NotNull(await new AccountRepository(_db.Create()).GetAccountByUsernameAsync("alice"));
    }

    [Fact]
    public async Task GetAccountByUsernameAsync_Unknown_ReturnsNull()
    {
        Assert.Null(await new AccountRepository(_db.Create()).GetAccountByUsernameAsync("ghost"));
    }

    [Fact]
    public async Task GetAccountByEmailAsync_ReturnsMatchingAccount()
    {
        var repository = new AccountRepository(_db.Create());
        await repository.AddAccountAsync(NewAccount("alice", "alice@test.com"));
        await repository.AddAccountAsync(NewAccount("bob", "bob@test.com"));

        var found = await new AccountRepository(_db.Create()).GetAccountByEmailAsync("bob@test.com");

        Assert.Equal("bob", found!.Username);
    }
}
