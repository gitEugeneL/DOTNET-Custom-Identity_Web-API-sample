using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Api.Constants;
using Api.Features.Common;
using Api.Features.GenerateCode;
using Api.Features.Login;
using Api.Features.UserInfo;
using Api.IntegrationTests.TestData;

namespace Api.IntegrationTests.Features;

public class UserInfoTests(ApiWebApplicationFactory factory)
    : IntegrationTestBase(factory), IClassFixture<ApiWebApplicationFactory>
{
    [Theory]
    [ClassData(typeof(UserData))]
    public async Task UserInfo_WithValidUserAndValidAccessToken_ReturnsOkAndInfoBody(TestUser user)
    {
        // Arrange
        await Factory.SeedUser(user);

        var loginResponse = await Client.PostAsJsonAsync(ApiPaths.Login, new LoginRequest(user.Email, user.Password));
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginOrRefreshResponse>();

        // Act
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);
        var response = await Client.GetAsync(ApiPaths.Info);

        // Assert 
        Assert.NotNull(loginResult);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        Assert.NotNull(response.Content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<UserInfoResponse>();
        Assert.NotNull(result);
        Assert.Equal(user.Email, result.Email, StringComparer.OrdinalIgnoreCase);
        Assert.False(result.IsConfirmLocked);
        Assert.False(result.IsEmailConfirmed);
        Assert.Null(result.ConfirmLockExpires);
    }

    [Theory]
    [ClassData(typeof(UserData))]
    public async Task UserInfo_WithValidUserAndValidAccessTokenAndConfirmedEmail_ReturnsOkAndInfoBody(TestUser user)
    {
        // Arrange
        user = user with { EmailConfirmed = true };
        await Factory.SeedUser(user);

        var loginResponse = await Client.PostAsJsonAsync(ApiPaths.Login, new LoginRequest(user.Email, user.Password));
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginOrRefreshResponse>();

        // Act
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);
        var response = await Client.GetAsync(ApiPaths.Info);

        // Assert
        Assert.NotNull(loginResult);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        Assert.NotNull(response.Content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<UserInfoResponse>();
        Assert.NotNull(result);
        Assert.Equal(user.Email, result.Email, StringComparer.OrdinalIgnoreCase);
        Assert.False(result.IsConfirmLocked);
        Assert.True(result.IsEmailConfirmed);
        Assert.Null(result.ConfirmLockExpires);
    }

    [Theory]
    [ClassData(typeof(UserData))]
    public async Task UserInfo_WitValidUserAndWithoutLogin_ReturnsUnauthorized(TestUser user)
    {
        // Arrange
        await Factory.SeedUser(user);

        // Act
        var response = await Client.GetAsync(ApiPaths.Info);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [ClassData(typeof(UserData))]
    public async Task UserInfo_WithValidUserAndConfirmLockout_ReturnsOkAndInfoBody(TestUser user)
    {
        // Arrange
        var codeMaxAttempts = Factory.GetRequiredConfig<int>("Authentication:Code.MaxAttempts");
        var lockoutMinutes = Factory.GetRequiredConfig<int>("Authentication:ConfirmLockout.Lifetime.Minutes");

        // seed user
        await Factory.SeedUser(user);

        // generate code (multiple attempts)
        for (var i = 0; i <= codeMaxAttempts; i++)
            await Client.PostAsJsonAsync(ApiPaths.GenerateCode, new GenerateCodeRequest(user.Email));


        var loginResponse = await Client.PostAsJsonAsync(ApiPaths.Login, new LoginRequest(user.Email, user.Password));
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginOrRefreshResponse>();

        // Act
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);
        var response = await Client.GetAsync(ApiPaths.Info);

        // Assert
        Assert.NotNull(loginResult);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        Assert.NotNull(response.Content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<UserInfoResponse>();

        Assert.NotNull(result);
        Assert.Equal(user.Email, result.Email, StringComparer.OrdinalIgnoreCase);
        Assert.True(result.IsConfirmLocked);
        Assert.False(result.IsEmailConfirmed);

        var lockoutDiff = Math.Abs((result.ConfirmLockExpires!.Value - DateTime.UtcNow.AddMinutes(lockoutMinutes))
            .TotalSeconds);
        Assert.True(lockoutDiff <= 3);
    }
}