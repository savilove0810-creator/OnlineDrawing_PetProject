using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Auth.Api.Data.Models;
using Auth.Api.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Drawing.Tests.Unit.AuthApi;

public class JwtServiceTests
{
    private const string Key = "0123456789abcdef0123456789abcdef";

    private static JwtService Create(Dictionary<string, string?> values) =>
        new(new ConfigurationBuilder().AddInMemoryCollection(values).Build());

    private static Dictionary<string, string?> BaseConfig() => new()
    {
        ["Jwt:Key"] = Key,
        ["Jwt:Issuer"] = "issuer",
        ["Jwt:Audience"] = "audience"
    };

    private static Account Account() => new() { Username = "alice", Email = "alice@test.com" };

    [Fact]
    public void GenerateToken_WithoutKey_Throws()
    {
        var service = Create(new());

        Assert.Throws<InvalidOperationException>(() => service.GenerateToken(Account()));
    }

    [Fact]
    public void GenerateToken_ProducesValidTokenWithClaims()
    {
        var account = Account();
        var token = Create(BaseConfig()).GenerateToken(account);

        var principal = new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
        {
            ValidIssuer = "issuer",
            ValidAudience = "audience",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key))
        }, out _);

        Assert.Equal(account.Id.ToString(), principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.Equal("alice", principal.FindFirst(ClaimTypes.Name)?.Value);
        Assert.Equal("alice@test.com", principal.FindFirst(ClaimTypes.Email)?.Value);
    }

    [Fact]
    public void GenerateToken_WithoutExpiration_DefaultsToSevenDays()
    {
        var token = Create(BaseConfig()).GenerateToken(Account());

        var validTo = new JwtSecurityTokenHandler().ReadJwtToken(token).ValidTo;
        Assert.InRange(validTo, DateTime.UtcNow.AddDays(7).AddMinutes(-1), DateTime.UtcNow.AddDays(7).AddMinutes(1));
    }

    [Fact]
    public void GenerateToken_ExpirationMinutesOverrideDays()
    {
        var config = BaseConfig();
        config["Jwt:ExpirationMinutes"] = "5";
        config["Jwt:ExpirationDays"] = "30";

        var token = Create(config).GenerateToken(Account());

        var validTo = new JwtSecurityTokenHandler().ReadJwtToken(token).ValidTo;
        Assert.InRange(validTo, DateTime.UtcNow.AddMinutes(4), DateTime.UtcNow.AddMinutes(6));
    }
}
