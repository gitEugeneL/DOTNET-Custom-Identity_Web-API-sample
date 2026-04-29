using Api.Features.ChangeEmail;
using Microsoft.Extensions.Configuration;
using Validator = Api.Features.ChangeEmail.Validator;

namespace Api.Tests.Features;

public class ChangeEmailRequestTests
{
    private readonly Validator _changeEmailRequestValidator;

    public ChangeEmailRequestTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Authentication:Code.Length", TestHelpers.CodeLength.ToString() }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _changeEmailRequestValidator = new Validator(configuration);
    }

    public static IEnumerable<object[]> ValidData()
    {
        yield return ["dev@dev.com", TestHelpers.GenerateValidCode(TestHelpers.CodeLength)];
        yield return ["test@test.com", TestHelpers.GenerateValidCode(TestHelpers.CodeLength)];
        yield return ["user@mail.com", TestHelpers.GenerateValidCode(TestHelpers.CodeLength)];
    }

    public static IEnumerable<object[]> InvalidData()
    {
        yield return ["", TestHelpers.GenerateValidCode(TestHelpers.CodeLength)]; // Empty Email
        yield return ["notAnEmail", TestHelpers.GenerateValidCode(TestHelpers.CodeLength)]; // Invalid Email format
        yield return ["test@test.com", ""]; // Empty Code
        yield return ["dev@test.com", "132246587645"]; // Invalid Code
        yield return ["dev@test.com", "132"]; // Invalid Code
    }

    [Theory]
    [MemberData(nameof(ValidData))]
    public async Task ValidChangePasswordRequest_PassesValidation(string newEmail, string code)
    {
        // Arrange
        var model = new ChangeEmailRequest(newEmail, code);

        // Act
        var result = await _changeEmailRequestValidator.ValidateAsync(model);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [MemberData(nameof(InvalidData))]
    public async Task InvalidChangePasswordRequest_FailsValidation(string newEmail, string code)
    {
        // Arrange
        var model = new ChangeEmailRequest(newEmail, code);

        // Act
        var result = await _changeEmailRequestValidator.ValidateAsync(model);

        // Assert
        Assert.False(result.IsValid);
    }
}