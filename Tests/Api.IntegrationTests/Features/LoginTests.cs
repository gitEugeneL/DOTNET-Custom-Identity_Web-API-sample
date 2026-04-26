using System.Net;
using System.Net.Http.Json;
using Api.Constants;
using Api.Domain.Enums;
using Api.Features.Common;
using Api.Features.Login;
using Api.IntegrationTests.TestData;
using Api.Tools;

namespace Api.IntegrationTests.Features;

public class LoginTests(ApiWebApplicationFactory factory)
    : IntegrationTestBase(factory), IClassFixture<ApiWebApplicationFactory>
{
    [Theory]
    [ClassData(typeof(UserData))]
    public async Task Login_WithValidData_ReturnsOkResultWitAccessTokenAndSetSecureCookieWithRefreshToken(TestUser user)
    {
        // Arrange
        var accessTokenMinutes = Factory.GetRequiredConfig<int>("Authentication:AccessToken.Lifetime.Minutes");
        var refreshTokenDays = Factory.GetRequiredConfig<int>("Authentication:RefreshToken.Lifetime.Days");

        var userId = await Factory.SeedUser(user);
        var request = new LoginRequest(user.Email, user.Password);

        // Act
        var response = await Client.PostAsJsonAsync(ApiPaths.Login, request);

        // Assert
        var result = await response.Content.ReadFromJsonAsync<LoginOrRefreshResponse>();
        var cookies = response.Headers.GetValues("Set-Cookie").ToList();

        Assert.NotNull(response.Content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.False(string.IsNullOrEmpty(result.AccessToken));

        var accessTokenDiff =
            Math.Abs((result.AccessTokenExpires - DateTime.UtcNow.AddMinutes(accessTokenMinutes)).TotalSeconds);
        Assert.True(accessTokenDiff <= 3);

        var refreshTokenDiff =
            Math.Abs((result.RefreshTokenExpires - DateTime.UtcNow.AddDays(refreshTokenDays)).TotalSeconds);
        Assert.True(refreshTokenDiff <= 3);

        var refreshTokenType = user.Role is Role.Admin
            ? CookieManager.AdminRefreshCookieName
            : CookieManager.CustomerRefreshCookieName;

        var refreshTokenCookie = cookies.FirstOrDefault(c => c.Contains(refreshTokenType));

        Assert.Contains(cookies, c => c.Contains(refreshTokenType));
        Assert.Contains(refreshTokenType + "=", refreshTokenCookie);
        Assert.Contains("secure", refreshTokenCookie);
        Assert.Contains("httponly", refreshTokenCookie);
        Assert.Contains("samesite=strict", refreshTokenCookie);
        Assert.Contains($"expires={DateTime.UtcNow.AddDays(refreshTokenDays):R}", refreshTokenCookie);
    }

    [Theory]
    [ClassData(typeof(UserData))]
    public async Task Login_WithInvalidUser_ReturnsBadRequestAndErrorMessage(TestUser user)
    {
        // Arrange
        var request = new LoginRequest(user.Email, user.Password);

        // Act
        var response = await Client.PostAsJsonAsync(ApiPaths.Login, request);

        // Assert
        Assert.NotNull(response.Content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<string>();

        Assert.NotNull(result);
        Assert.Equal(ApiMessages.InvalidAuth, result);
    }

    [Theory]
    [ClassData(typeof(UserData))]
    public async Task Login_WithValidUserAndInvalidPassword_ReturnsBadRequestAndErrorMessage(TestUser user)
    {
        // Arrange
        await Factory.SeedUser(user);

        user = user with { Password = "invalidPassword123!" };
        var request = new LoginRequest(user.Email, user.Password);

        // Act
        var response = await Client.PostAsJsonAsync(ApiPaths.Login, request);

        // Assert
        Assert.NotNull(response.Content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<string>();

        Assert.NotNull(result);
        Assert.Equal(ApiMessages.InvalidAuth, result);
    }

    [Theory]
    [ClassData(typeof(UserData))]
    public async Task Login_WithValidUserAndMultipleInvalidAttempts_ReturnsBadRequestAndErrorMessage(TestUser user)
    {
        // Arrange
        var loginMaxAttempts = Factory.GetRequiredConfig<int>("Authentication:LoginLockout.MaxAttempts");

        await Factory.SeedUser(user);

        for (var i = 0; i <= loginMaxAttempts; i++)
            await Client.PostAsJsonAsync(ApiPaths.Login, new LoginRequest(user.Email, "invalidPwd123!"));

        var request = new LoginRequest(user.Email, user.Password);

        // Act
        var response = await Client.PostAsJsonAsync(ApiPaths.Login, request);

        // Assert
        Assert.NotNull(response.Content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<string>();

        Assert.NotNull(result);

        Assert.Equal(ApiMessages.InvalidAuth, result);
    }
}