using Api.Features.ConfirmEmail;
using Microsoft.Extensions.Configuration;

namespace Api.Tests.Features;

public class ConfirmEmailTests
{
    private readonly Validator _confirmEmailRequestValidator;

    public ConfirmEmailTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Authentication:Code.Length", TestHelpers.CodeLength.ToString() }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _confirmEmailRequestValidator = new Validator(configuration);
    }

    public static IEnumerable<object[]> ValidData()
    {
        yield return [TestHelpers.GenerateValidCode(TestHelpers.CodeLength), "dev@dev.com"];
        yield return [TestHelpers.GenerateValidCode(TestHelpers.CodeLength), "test@test.com"];
        yield return [TestHelpers.GenerateValidCode(TestHelpers.CodeLength), "user@mail.com"];
    }

    public static IEnumerable<object[]> InvalidData()
    {
        yield return ["", "test@test.com"]; // Empty Code
        yield return [TestHelpers.GenerateValidCode(TestHelpers.CodeLength), ""]; // Empty Email
        yield return [TestHelpers.GenerateValidCode(TestHelpers.CodeLength), "notanemail"]; // Invalid Email
        yield return ["25", "dev@dev.com"]; // Invalid Code
        yield return ["212312312312", "de123v@dev.com"]; // Invalid Code
    }


    [Theory]
    [MemberData(nameof(ValidData))]
    public async Task ValidConfirmEmailRequest_PassesValidation(string code, string email)
    {
        // Arrange
        var model = new ConfirmEmailRequest(code, email);

        // Act
        var result = await _confirmEmailRequestValidator.ValidateAsync(model);

        // Arrange
        Assert.True(result.IsValid);
    }

    [Theory]
    [MemberData(nameof(InvalidData))]
    public async Task InvalidConfirmEmailRequest_FailsValidation(string code, string email)
    {
        // Arrange
        var model = new ConfirmEmailRequest(code, email);

        // Act
        var result = await _confirmEmailRequestValidator.ValidateAsync(model);

        // Arrange
        Assert.False(result.IsValid);
    }
}