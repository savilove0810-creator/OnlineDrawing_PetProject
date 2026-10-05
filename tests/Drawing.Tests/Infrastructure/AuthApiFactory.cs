using Auth.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AuthDb = Auth.Api.Data.AppDbContext;

namespace Drawing.Tests.Infrastructure;

public class AuthApiFactory : WebApplicationFactory<JwtService>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public AuthApiFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:Key", JwtCreator.Key);
        builder.UseSetting("ConnectionStrings:DefaultConnection", "Host=unused");
        builder.ConfigureServices(services => services.ReplaceWithSqlite<AuthDb>(_connection));
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        host.Services.EnsureDatabase<AuthDb>();
        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _connection.Dispose();
    }
}
