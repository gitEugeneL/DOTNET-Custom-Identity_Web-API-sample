using Api.Services;
using Microsoft.Extensions.Configuration;

namespace Api.Tests.Services;

public class ConfirmationServiceTests
{
    private readonly IConfiguration _configuration;

    public ConfirmationServiceTests()
    {
        var configurationSettings = new Dictionary<string, string?>
        {
            { "Authentication:Code.Length", TestHelpers.CodeLength.ToString() },
            { "Authentication:Code.Lifetime.Minutes", TestHelpers.CodeLifeTimeMinutes.ToString() }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationSettings)
            .Build();
    }

    [Fact]
    public void GenerateCode_ReturnsCodeWithCorrectLength()
    {
        // Arrange
        var service = new ConfirmationService(_configuration);

        // Act
        var (code, _) = service.GenerateCode();

        // Assert
        Assert.True(code.All(char.IsDigit));
        Assert.True(code.Length == TestHelpers.CodeLength);
    }

    [Fact]
    public void GenerateCode_ReturnExpirationTimeInFuture()
    {
        // Arrange
        var service = new ConfirmationService(_configuration);

        // Act
        var (_, expires) = service.GenerateCode();

        // Assert
        var difference = Math.Abs((expires - DateTime.UtcNow.AddMinutes(TestHelpers.CodeLifeTimeMinutes)).TotalSeconds);
        Assert.True(difference < 3);
    }
}