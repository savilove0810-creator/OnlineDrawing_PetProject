using System.ComponentModel.DataAnnotations;
using Auth.Api.Data.DTOs;

namespace Drawing.Tests.Unit.AuthApi;

public class DtoValidationTests
{
    private static bool IsValid(object dto) =>
        Validator.TryValidateObject(dto, new ValidationContext(dto), new List<ValidationResult>(), true);

    [Fact]
    public void RegisterDto_Valid_Passes()
    {
        Assert.True(IsValid(new RegisterDto { Username = "alice", Email = "a@test.com", Password = "secret1" }));
    }

    [Theory]
    [InlineData("ab", "a@test.com", "secret1")]
    [InlineData("", "a@test.com", "secret1")]
    [InlineData("alice", "not-an-email", "secret1")]
    [InlineData("alice", "a@test.com", "12345")]
    [InlineData("alice", "a@test.com", "")]
    public void RegisterDto_Invalid_Fails(string username, string email, string password)
    {
        Assert.False(IsValid(new RegisterDto { Username = username, Email = email, Password = password }));
    }

    [Fact]
    public void RegisterDto_TooLongUsername_Fails()
    {
        Assert.False(IsValid(new RegisterDto { Username = new string('a', 51), Email = "a@test.com", Password = "secret1" }));
    }

    [Theory]
    [InlineData("", "pass")]
    [InlineData("alice", "")]
    public void LoginDto_MissingFields_Fails(string username, string password)
    {
        Assert.False(IsValid(new LoginDto { Username = username, Password = password }));
    }

    [Fact]
    public void LoginDto_Valid_Passes()
    {
        Assert.True(IsValid(new LoginDto { Username = "alice", Password = "pass" }));
    }
}
