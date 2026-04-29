using Api.Features.Login;
using FluentValidation.TestHelper;

namespace Api.Tests.Features;

public class LoginRequestTests
{
    private readonly Validator _loginRequestValidator = new();

    [Theory]
    [InlineData("test@example.com", "strongPassword1@")]
    [InlineData("mail@example.com", "devDev123!^%$")]
    public async Task ValidLoginRequest_PassesValidation(string email, string password)
    {
        // Arrange
        var model = new LoginRequest(email, password);

        // Act
        var result = await _loginRequestValidator.TestValidateAsync(model);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", "password")] // Empty Email
    [InlineData("test@example.com", "")] // Empty Password
    [InlineData("notanemail", "strongPassword1@")] // Invalid Email format
    [InlineData("test@example.com", "sh!23!")] // Password too short
    public async Task InvalidLoginRequest_FailsValidation(string email, string password)
    {
        // Arrange
        var model = new LoginRequest(email, password);

        // Act
        var result = await _loginRequestValidator.TestValidateAsync(model);

        // Assert
        Assert.False(result.IsValid);
    }
}