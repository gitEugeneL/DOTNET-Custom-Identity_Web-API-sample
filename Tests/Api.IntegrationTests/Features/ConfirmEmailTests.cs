using System.Net;
using System.Net.Http.Json;
using Api.Constants;
using Api.Features.ConfirmEmail;
using Api.Features.GenerateCode;
using Api.IntegrationTests.FakeServices;
using Api.IntegrationTests.TestData;

namespace Api.IntegrationTests.Features;

public class ConfirmEmailTests(ApiWebApplicationFactory factory)
    : IntegrationTestBase(factory), IClassFixture<ApiWebApplicationFactory>
{
    [Theory]
    [ClassData(typeof(UserData))]
    public async Task ConfirmEmail_WithValidCode_ReturnsOkResultWithResultResponse(TestUser user)
    {
        // Arrange
        var codeLength = Factory.GetRequiredConfig<int>("Authentication:Code.Length");

        // seed user
        await Factory.SeedUser(user);

        // generate code
        await Client.PostAsJsonAsync(ApiPaths.GenerateCode, new GenerateCodeRequest(user.Email));

        // email confirmation code
        var fakeValidCode =
            new string(Enumerable.Repeat(FakeConfirmationService.ValidFakeCodeChar, codeLength).ToArray());

        var request = new ConfirmEmailRequest(fakeValidCode, user.Email);

        // Act
        var response = await Client.PostAsJsonAsync(ApiPaths.ConfirmEmail, request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ConfirmEmailResponse>();

        Assert.NotNull(result);
        Assert.Equal(result.Email, user.Email, StringComparer.OrdinalIgnoreCase);
        Assert.True(result.IsConfirmed);
    }

    [Theory]
    [ClassData(typeof(UserData))]
    public async Task ConfirmEmail_WithValidUserAndInvalidCode_ReturnsBadRequestAndErrorMessage(TestUser user)
    {
        // Arrange 
        var codeLength = Factory.GetRequiredConfig<int>("Authentication:Code.Length");

        // seed user
        await Factory.SeedUser(user);

        // generate code
        await Client.PostAsJsonAsync(ApiPaths.GenerateCode, new GenerateCodeRequest(user.Email));

        // email confirmation code
        var fakeInvalidCode =
            new string(Enumerable.Repeat(FakeConfirmationService.InvalidFakeCodeChar, codeLength).ToArray());

        var request = new ConfirmEmailRequest(fakeInvalidCode, user.Email);

        // Act
        var response = await Client.PostAsJsonAsync(ApiPaths.ConfirmEmail, request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<string>();

        Assert.NotNull(result);
        Assert.Equal(ApiMessages.InvalidConfirm, result);
    }

    [Theory]
    [ClassData(typeof(UserData))]
    public async Task ConfirmEmail_WithValidUserAndValidCodeAndAlreadyConfirmedEmail_ReturnsBadRequestAndErrorMessage(
        TestUser user)
    {
        // Arrange
        var codeLength = Factory.GetRequiredConfig<int>("Authentication:Code.Length");

        // seed user
        user = user with { EmailConfirmed = true };
        await Factory.SeedUser(user);

        // generate code
        await Client.PostAsJsonAsync(ApiPaths.GenerateCode, new GenerateCodeRequest(user.Email));

        // email confirmation code
        var fakeValidCode =
            new string(Enumerable.Repeat(FakeConfirmationService.ValidFakeCodeChar, codeLength).ToArray());

        var request = new ConfirmEmailRequest(fakeValidCode, user.Email);

        // Act
        var response = await Client.PostAsJsonAsync(ApiPaths.ConfirmEmail, request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<string>();

        Assert.NotNull(result);
        Assert.Equal(ApiMessages.InvalidConfirm, result);
    }

    [Theory]
    [ClassData(typeof(UserData))]
    public async Task ConfirmEmail_WithValidUserAndWithoutGenerateCode_ReturnsBadRequestAndErrorMessage(TestUser user)
    {
        // Arrange
        var codeLength = Factory.GetRequiredConfig<int>("Authentication:Code.Length");

        // seed user
        await Factory.SeedUser(user);

        // email confirmation code
        var fakeValidCode =
            new string(Enumerable.Repeat(FakeConfirmationService.ValidFakeCodeChar, codeLength).ToArray());

        var request = new ConfirmEmailRequest(fakeValidCode, user.Email);

        // Act
        var response = await Client.PostAsJsonAsync(ApiPaths.ConfirmEmail, request);

        // Assert 
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<string>();

        Assert.NotNull(result);
        Assert.Equal(ApiMessages.InvalidConfirm, result);
    }

    [Theory]
    [ClassData(typeof(UserData))]
    public async Task ConfirmEmail_WithInvalidUser_ReturnsBadRequestAndErrorMessage(TestUser user)
    {
        // Arrange
        var codeLength = Factory.GetRequiredConfig<int>("Authentication:Code.Length");

        // email confirmation code
        var fakeValidCode =
            new string(Enumerable.Repeat(FakeConfirmationService.ValidFakeCodeChar, codeLength).ToArray());

        var request = new ConfirmEmailRequest(fakeValidCode, user.Email);

        // Act
        var response = await Client.PostAsJsonAsync(ApiPaths.ConfirmEmail, request);

        // Assert 
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<string>();

        Assert.NotNull(result);
        Assert.Equal(ApiMessages.InvalidConfirm, result);
    }

    [Theory]
    [ClassData(typeof(UserData))]
    public async Task ConfirmEmail_WithLockoutUserAndValidCode_ReturnsBadRequestAndErrorMessage(TestUser user)
    {
        // Arrange
        var codeLength = Factory.GetRequiredConfig<int>("Authentication:Code.Length");
        var codeMaxAttempts = Factory.GetRequiredConfig<int>("Authentication:Code.MaxAttempts");

        // seed user
        await Factory.SeedUser(user);

        // generate code (multiple attempts)
        for (var i = 0; i <= codeMaxAttempts; i++)
            await Client.PostAsJsonAsync(ApiPaths.GenerateCode, new GenerateCodeRequest(user.Email));

        // email confirmation code
        var fakeValidCode =
            new string(Enumerable.Repeat(FakeConfirmationService.ValidFakeCodeChar, codeLength).ToArray());

        var request = new ConfirmEmailRequest(fakeValidCode, user.Email);

        // Act
        var response = await Client.PostAsJsonAsync(ApiPaths.ConfirmEmail, request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<string>();

        Assert.NotNull(result);
        Assert.Equal(ApiMessages.InvalidConfirm, result);
    }
}