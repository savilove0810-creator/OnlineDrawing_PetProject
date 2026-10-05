using Drawing.Shared.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Drawing.Tests.Unit.SharedWeb;

public class SharedWebTests
{
    private static IConfiguration Config(Dictionary<string, string?> values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    [Fact]
    public void AddJwtAuthentication_WithoutKey_Throws()
    {
        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(() => services.AddJwtAuthentication(Config(new())));
    }

    [Fact]
    public void AddJwtAuthentication_WithKey_ConfiguresValidation()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddJwtAuthentication(Config(new()
        {
            ["Jwt:Key"] = "0123456789abcdef0123456789abcdef",
            ["Jwt:Issuer"] = "issuer",
            ["Jwt:Audience"] = "audience"
        }));

        var options = services.BuildServiceProvider()
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        Assert.Equal("issuer", options.TokenValidationParameters.ValidIssuer);
        Assert.Equal("audience", options.TokenValidationParameters.ValidAudience);
        Assert.True(options.TokenValidationParameters.ValidateLifetime);
    }

    [Fact]
    public async Task UseApiExceptionHandling_OnException_Returns400WithMessage()
    {
        var builder = new ApplicationBuilder(new ServiceCollection().BuildServiceProvider());
        builder.UseApiExceptionHandling();
        builder.Run(_ => throw new Exception("boom"));
        var pipeline = builder.Build();

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await pipeline(context);

        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal("boom", body);
    }

    [Fact]
    public async Task UseApiExceptionHandling_WithoutException_PassesThrough()
    {
        var builder = new ApplicationBuilder(new ServiceCollection().BuildServiceProvider());
        builder.UseApiExceptionHandling();
        builder.Run(context =>
        {
            context.Response.StatusCode = StatusCodes.Status204NoContent;
            return Task.CompletedTask;
        });
        var pipeline = builder.Build();

        var context = new DefaultHttpContext();
        await pipeline(context);

        Assert.Equal(StatusCodes.Status204NoContent, context.Response.StatusCode);
    }
}
