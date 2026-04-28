using System.Net;
using System.Net.Http.Json;
using Api.Constants;
using Api.Features.GenerateCode;
using Api.IntegrationTests.TestData;

namespace Api.IntegrationTests.Features;

public class GenerateCodeTests(ApiWebApplicationFactory factory)
    : IntegrationTestBase(factory), IClassFixture<ApiWebApplicationFactory>
{
    [Theory]
    [ClassData(typeof(UserData))]
    public async Task GenerateCode_WithValidUser_CrateCodeAndReturnsOkResult(TestUser user)
    {
        // Arrange
        var codeLifeTimeMinutes = Factory.GetRequiredConfig<int>("Authentication:Code.Lifetime.Minutes");

        await Factory.SeedUser(user);
        var request = new GenerateCodeRequest(user.Email);

        // Act
        var response = await Client.PostAsJsonAsync(ApiPaths.GenerateCode, request);

        // Assert
        Assert.NotNull(response.Content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GenerateCodeResponse>();

        Assert.NotNull(result);
        Assert.Equal(result.Email, user.Email, StringComparer.OrdinalIgnoreCase);

        var timeDifference =
            Math.Abs((result.CodeExpires - DateTime.UtcNow.AddMinutes(codeLifeTimeMinutes)).TotalSeconds);
        Assert.True(timeDifference <= 3);
    }

    [Theory]
    [ClassData(typeof(UserData))]
    public async Task GenerateCode_WithInvalidUser_ReturnsErrorMessageAndBadRequestResult(TestUser user)
    {
        // Arrange
        var request = new GenerateCodeRequest(user.Email);

        // Act
        var response = await Client.PostAsJsonAsync(ApiPaths.GenerateCode, request);

        // Assert
        Assert.NotNull(response.Content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<string>();

        Assert.NotNull(result);
        Assert.Equal(ApiMessages.InvalidConfirm, result);
    }

    [Theory]
    [ClassData(typeof(UserData))]
    public async Task GenerateCode_WithValidUserAndMultipleAttempts_ReturnsBadRequestAndErrorMessage(
        TestUser user)
    {
        // Arrange
        var generateCodeMaxAttempts = Factory.GetRequiredConfig<int>("Authentication:Code.MaxAttempts");

        await Factory.SeedUser(user);

        // Act
        var response = new HttpResponseMessage();
        for (var i = 0; i <= generateCodeMaxAttempts; i++)
            response = await Client.PostAsJsonAsync(ApiPaths.GenerateCode, new GenerateCodeRequest(user.Email));

        // Assert
        Assert.NotNull(response.Content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<string>();

        Assert.NotNull(result);
        Assert.Equal(ApiMessages.InvalidConfirm, result);
    }
}