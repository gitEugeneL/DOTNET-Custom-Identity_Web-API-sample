using Api.Features.Registration;
using FluentValidation.TestHelper;

namespace Api.Tests.Features;

public class RegistrationRequestTests
{
    private readonly Validator _registrationRequestValidator = new();

    [Theory]
    [InlineData("test@example.com", "StrongPassword1!", "StrongPassword1!")]
    [InlineData("user@domain.com", "AnotherPass123@", "AnotherPass123@")]
    public async Task ValidRegistrationRequest_PassesValidation(string email, string password, string confirmPassword)
    {
        // Arrange
        var model = new RegistrationRequest(email, password, confirmPassword);

        // Act
        var result = await _registrationRequestValidator.TestValidateAsync(model);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", "StrongPassword1!", "StrongPassword1!")] // Empty Email
    [InlineData("test@example.com", "", "StrongPassword1!")] // Empty Password
    [InlineData("test@example.com", "StrongPassword1!", "")] // Empty ConfirmPassword
    [InlineData("test@example.com", "StrongPassword1!", "WrongPassword1!")] // Mismatched Passwords
    [InlineData("notanemail", "StrongPassword1!", "StrongPassword1!")] // Invalid Email format
    [InlineData("test@example.com", "short", "short")] // Password too short
    [InlineData("test@example.com", "password", "password")] // Password without uppercase
    [InlineData("test@example.com", "PASSWORD", "PASSWORD")] // Password without lowercase
    [InlineData("test@example.com", "Password", "Password")] // Password without digits
    [InlineData("test@example.com", "Password1", "Password1")] // Password without special characters
    public async Task InvalidRegistrationRequest_FailsValidation(string email, string password, string confirmPassword)
    {
        // Arrange
        var model = new RegistrationRequest(email, password, confirmPassword);


        // Act
        var result = await _registrationRequestValidator.TestValidateAsync(model);

        // Assert
        Assert.False(result.IsValid);
    }
}