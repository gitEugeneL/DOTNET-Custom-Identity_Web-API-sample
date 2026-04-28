using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Api.Constants;
using Api.Features.ChangeEmail;
using Api.Features.Common;
using Api.Features.GenerateCode;
using Api.Features.Login;
using Api.IntegrationTests.FakeServices;
using Api.IntegrationTests.TestData;

namespace Api.IntegrationTests.Features;

public class ChangeEmailTests(ApiWebApplicationFactory factory)
    : IntegrationTestBase(factory), IClassFixture<ApiWebApplicationFactory>
{
    [Theory]
    [ClassData(typeof(UserData))]
    public async Task ChangeEmail_WithValidUserAndValidCode_ReturnsOkAndResetResult(TestUser user)
    {
        // Arrange
        var codeLength = Factory.GetRequiredConfig<int>("Authentication:Code.Length");

        // seed user
        user = user with { EmailConfirmed = true };
        await Factory.SeedUser(user);

        var newEmail = "new-test-email-" + user.Email;

        // generate code
        await Client.PostAsJsonAsync(ApiPaths.GenerateCode, new GenerateCodeRequest(user.Email));

        // confirmation code (fake code)
        var validCode = new string(Enumerable.Repeat(FakeConfirmationService.ValidFakeCode, codeLength).ToArray());

        var loginResponse = await Client.PostAsJsonAsync(ApiPaths.Login, new LoginRequest(user.Email, user.Password));
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginOrRefreshResponse>();

        // Act
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);
        var request = new ChangeEmailRequest(newEmail, validCode);
        var response = await Client.PostAsJsonAsync(ApiPaths.ChangeEmail, request);

        // Assert
        Assert.NotNull(response.Content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ChangeEmailResponse>();

        Assert.NotNull(result);
        Assert.Equal(newEmail, result.NewEmail, StringComparer.OrdinalIgnoreCase);
        Assert.True(result.IsEmailChanged);
    }

    [Theory]
    [ClassData(typeof(UserData))]
    public async Task ChangeEmail_WithValidUserAndInvalidCode_ReturnsBadRequestAndErrorMessage(TestUser user)
    {
        // Arrange
        var codeLength = Factory.GetRequiredConfig<int>("Authentication:Code.Length");

        // seed user
        user = user with { EmailConfirmed = true };
        await Factory.SeedUser(user);

        var newEmail = "new-test-email-" + user.Email;

        // generate code
        await Client.PostAsJsonAsync(ApiPaths.GenerateCode, new GenerateCodeRequest(user.Email));

        // confirmation code (fake code)
        var invalidCode = new string(Enumerable.Repeat(FakeConfirmationService.InvalidFakeCode, codeLength).ToArray());

        var loginResponse = await Client.PostAsJsonAsync(ApiPaths.Login, new LoginRequest(user.Email, user.Password));
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginOrRefreshResponse>();

        // Act
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);
        var request = new ChangeEmailRequest(newEmail, invalidCode);
        var response = await Client.PostAsJsonAsync(ApiPaths.ChangeEmail, request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<string>();

        Assert.NotNull(result);
        Assert.Equal(ApiMessages.InvalidConfirm, result);
    }

    [Theory]
    [ClassData(typeof(UserData))]
    public async Task ChangeEmail_WithValidUserAndUnconfirmedEmail_ReturnsBadRequestAndErrorMessage(TestUser user)
    {
        // Arrange
        var codeLength = Factory.GetRequiredConfig<int>("Authentication:Code.Length");

        // seed user
        await Factory.SeedUser(user);

        var newEmail = "new-test-email-" + user.Email;

        // generate code
        await Client.PostAsJsonAsync(ApiPaths.GenerateCode, new GenerateCodeRequest(user.Email));

        // confirmation code (fake code)
        var validCode = new string(Enumerable.Repeat(FakeConfirmationService.ValidFakeCode, codeLength).ToArray());

        var loginResponse = await Client.PostAsJsonAsync(ApiPaths.Login, new LoginRequest(user.Email, user.Password));
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginOrRefreshResponse>();

        // Act
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);
        var request = new ChangeEmailRequest(newEmail, validCode);
        var response = await Client.PostAsJsonAsync(ApiPaths.ChangeEmail, request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<string>();

        Assert.NotNull(result);
        Assert.Equal(ApiMessages.InvalidConfirm, result);
    }

    [Theory]
    [ClassData(typeof(UserData))]
    public async Task ChangeEmail_WithValidUserAndWithoutGenerateCode_ReturnsBadRequestAndErrorMessage(TestUser user)
    {
        // Arrange
        var codeLength = Factory.GetRequiredConfig<int>("Authentication:Code.Length");

        // seed user
        user = user with { EmailConfirmed = true };
        await Factory.SeedUser(user);

        var newEmail = "new-test-email-" + user.Email;

        // confirmation code (fake code)
        var code = new string(Enumerable.Repeat(FakeConfirmationService.ValidFakeCode, codeLength).ToArray());

        var loginResponse = await Client.PostAsJsonAsync(ApiPaths.Login, new LoginRequest(user.Email, user.Password));
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginOrRefreshResponse>();

        // Act
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);
        var request = new ChangeEmailRequest(newEmail, code);
        var response = await Client.PostAsJsonAsync(ApiPaths.ChangeEmail, request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<string>();

        Assert.NotNull(result);
        Assert.Equal(ApiMessages.InvalidConfirm, result);
    }

    [Theory]
    [ClassData(typeof(UserData))]
    public async Task ChangeEmail_WithValidUserAndWithoutLogin_ReturnsUnauthorized(TestUser user)
    {
        // Arrange
        var codeLength = Factory.GetRequiredConfig<int>("Authentication:Code.Length");

        // seed user
        user = user with { EmailConfirmed = true };
        await Factory.SeedUser(user);

        var newEmail = "new-test-email-" + user.Email;

        // confirmation code (fake code)
        var code = new string(Enumerable.Repeat(FakeConfirmationService.ValidFakeCode, codeLength).ToArray());

        // Act
        var request = new ChangeEmailRequest(newEmail, code);
        var response = await Client.PostAsJsonAsync(ApiPaths.ChangeEmail, request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [ClassData(typeof(UserData))]
    public async Task ChangeEmail_WithLockoutUserAndValidCode_ReturnsBadRequestAndErrorMessage(TestUser user)
    {
        // Arrange
        var codeLength = Factory.GetRequiredConfig<int>("Authentication:Code.Length");
        var codeMaxAttempts = Factory.GetRequiredConfig<int>("Authentication:Code.MaxAttempts");

        // seed user
        user = user with { EmailConfirmed = true };
        await Factory.SeedUser(user);

        var newEmail = "new-test-email-" + user.Email;

        // generate code (multiple attempts)
        for (var i = 0; i <= codeMaxAttempts; i++)
            await Client.PostAsJsonAsync(ApiPaths.GenerateCode, new GenerateCodeRequest(user.Email));

        // confirmation code (fake code)
        var validCode = new string(Enumerable.Repeat(FakeConfirmationService.ValidFakeCode, codeLength).ToArray());

        var loginResponse = await Client.PostAsJsonAsync(ApiPaths.Login, new LoginRequest(user.Email, user.Password));
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginOrRefreshResponse>();

        // Act
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);
        var request = new ChangeEmailRequest(newEmail, validCode);
        var response = await Client.PostAsJsonAsync(ApiPaths.ChangeEmail, request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<string>();

        Assert.NotNull(result);
        Assert.Equal(ApiMessages.InvalidConfirm, result);
    }
}