using Api.Features.ChangePassword;
using Microsoft.Extensions.Configuration;

namespace Api.Tests.Features;

public class ChangePasswordRequestTests
{
    private readonly Validator _changePasswordRequestValidator;

    public ChangePasswordRequestTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Authentication:Code.Length", TestHelpers.CodeLength.ToString() }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _changePasswordRequestValidator = new Validator(configuration);
    }

    public static IEnumerable<object[]> ValidData()
    {
        yield return
        [
            "dev@dev.com", TestHelpers.GenerateValidCode(TestHelpers.CodeLength), "StrongPassword1!", "StrongPassword1!"
        ];
        yield return
        [
            "test@test.com", TestHelpers.GenerateValidCode(TestHelpers.CodeLength), "AnotherPass123@", "AnotherPass123@"
        ];
    }

    public static IEnumerable<object[]> InvalidData()
    {
        yield return // Empty Email
        [
            "", TestHelpers.GenerateValidCode(TestHelpers.CodeLength), "StrongPassword1!", "StrongPassword1!"
        ];
        yield return // Empty Password
        [
            "test@example.com", TestHelpers.GenerateValidCode(TestHelpers.CodeLength), "", "StrongPassword1!"
        ];
        yield return // Empty ConfirmPassword
        [
            "test@example.com", TestHelpers.GenerateValidCode(TestHelpers.CodeLength), "StrongPassword1!", ""
        ];
        yield return // Mismatched Passwords
        [
            "test@example.com", TestHelpers.GenerateValidCode(TestHelpers.CodeLength), "StrongPaswd!", "WrongPassword1!"
        ];
        yield return // Invalid Email format
        [
            "notanemail", TestHelpers.GenerateValidCode(TestHelpers.CodeLength), "StrongPassword1!", "StrongPassword1!"
        ];
        yield return // Password too short
        [
            "test@example.com", TestHelpers.GenerateValidCode(TestHelpers.CodeLength), "short", "short"
        ];
        yield return // Password without uppercase
        [
            "test@example.com", TestHelpers.GenerateValidCode(TestHelpers.CodeLength), "password", "password"
        ];
        yield return // Password without lowercase
        [
            "test@example.com", TestHelpers.GenerateValidCode(TestHelpers.CodeLength), "PASSWORD", "PASSWORD"
        ];
        yield return // Password without digits
        [
            "test@example.com", TestHelpers.GenerateValidCode(TestHelpers.CodeLength), "Password", "Password"
        ];
        yield return // Password without special characters
        [
            "test@example.com", TestHelpers.GenerateValidCode(TestHelpers.CodeLength), "Password1", "Password1"
        ];
        yield return ["test@test.com", "", "StrongPassword1!", "StrongPassword1!"]; // Empty Code
        yield return ["dev@test.com", "132246587645", "AnotherPass123@", "AnotherPass123@"]; // Invalid Code
        yield return ["dev@test.com", "132", "AnotherPass123@", "AnotherPass123@"]; // Invalid Code
    }

    [Theory]
    [MemberData(nameof(ValidData))]
    public async Task ValidResetPasswordRequest_PassesValidation(
        string email,
        string code,
        string password,
        string confirmPassword)
    {
        // Arrange
        var model = new ChangePasswordRequest(email, code, password, confirmPassword);

        // Act
        var result = await _changePasswordRequestValidator.ValidateAsync(model);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [MemberData(nameof(InvalidData))]
    public async Task InvalidResetPasswordRequest_FailsValidation(
        string email,
        string code,
        string password,
        string confirmPassword)
    {
        // Arrange
        var model = new ChangePasswordRequest(email, code, password, confirmPassword);

        // Act
        var result = await _changePasswordRequestValidator.ValidateAsync(model);

        // Assert
        Assert.False(result.IsValid);
    }
}