using Api.Features.GenerateCode;

namespace Api.Tests.Features;

public class GenerateCodeRequestTests
{
    private readonly Validator _generateCodeRequestValidator = new();

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("mail@example.com")]
    public async Task ValidGenerateCodeRequest_PassesValidation(string email)
    {
        // Arrange
        var model = new GenerateCodeRequest(email);

        // Act
        var result = await _generateCodeRequestValidator.ValidateAsync(model);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")] // Empty Email
    [InlineData("notanemail")] // Invalid Email form
    public async Task InvalidGenerateCodeCommand_FailsValidation(string email)
    {
        // Arrange
        var model = new GenerateCodeRequest(email);

        // Act
        var result = await _generateCodeRequestValidator.ValidateAsync(model);

        // Assert
        Assert.False(result.IsValid);
    }
}