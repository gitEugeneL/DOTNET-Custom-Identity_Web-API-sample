using System.Net;
using System.Net.Http.Json;
using Api.Constants;
using Api.Domain.Enums;
using Api.Features.Common;
using Api.IntegrationTests.TestData;
using Api.Tools;
using LoginRequest = Api.Features.Login.LoginRequest;

namespace Api.IntegrationTests.Features;

public class RefreshTests(ApiWebApplicationFactory factory)
    : IntegrationTestBase(factory), IClassFixture<ApiWebApplicationFactory>
{
    [Theory]
    [ClassData(typeof(UserData))]
    public async Task Refresh_WithValidUserAndRefreshToken_ReturnsOkWitAccessTokenAndSetRefreshToken(TestUser user)
    {
        // Arrange
        var accessTokenMinutes = Factory.GetRequiredConfig<int>("Authentication:AccessToken.Lifetime.Minutes");
        var refreshTokenDays = Factory.GetRequiredConfig<int>("Authentication:RefreshToken.Lifetime.Days");

        user = user with { EmailConfirmed = true };
        var userId = await Factory.SeedUser(user);

        var loginResponse = await Client.PostAsJsonAsync(ApiPaths.Login, new LoginRequest(user.Email, user.Password));

        // read cookie
        var userCookies = loginResponse.Headers.GetValues("Set-Cookie").ToList();
        var refreshTokenType = user.Role is Role.Admin
            ? CookieManager.AdminRefreshCookieName
            : CookieManager.CustomerRefreshCookieName;
        var userRefreshToken = userCookies.FirstOrDefault(c => c.Contains(refreshTokenType));

        // Act
        Client.DefaultRequestHeaders.Add("Cookie", userRefreshToken);
        var response = await Client.PostAsJsonAsync(ApiPaths.Refresh,
            new RefreshOrLogoutRequest(userId.ToString()!, user.Role.ToString()));
        
        // Assert
        Assert.NotNull(response.Content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<LoginOrRefreshResponse>();
        var cookies = response.Headers.GetValues("Set-Cookie").ToList();

        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.False(string.IsNullOrEmpty(result.AccessToken));

        var accessTokenDiff =
            Math.Abs((result.AccessTokenExpires - DateTime.UtcNow.AddMinutes(accessTokenMinutes)).TotalSeconds);
        Assert.True(accessTokenDiff <= 3);

        var refreshTokenDiff =
            Math.Abs((result.RefreshTokenExpires - DateTime.UtcNow.AddDays(refreshTokenDays)).TotalSeconds);
        Assert.True(refreshTokenDiff <= 3);

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
    public async Task Refresh_WithValidUserAndUsedRefreshToken_ReturnsBadRequestAndErrorMessage(TestUser user)
    {
        // Arrange
        user = user with { EmailConfirmed = true };
        var userId = await Factory.SeedUser(user);

        var loginResponse = await Client.PostAsJsonAsync(ApiPaths.Login, new LoginRequest(user.Email, user.Password));
        // read cookie
        var userCookies = loginResponse.Headers.GetValues("Set-Cookie").ToList();
        var refreshTokenType = user.Role is Role.Admin
            ? CookieManager.AdminRefreshCookieName
            : CookieManager.CustomerRefreshCookieName;
        var userRefreshToken = userCookies.FirstOrDefault(c => c.Contains(refreshTokenType));

        Client.DefaultRequestHeaders.Add("Cookie", userRefreshToken);
        await Client.PostAsJsonAsync(ApiPaths.Refresh,
            new RefreshOrLogoutRequest(userId.ToString()!, user.Role.ToString()));

        // Act
        Client.DefaultRequestHeaders.Add("Cookie", userRefreshToken);
        var response = await Client.PostAsJsonAsync(ApiPaths.Refresh,
            new RefreshOrLogoutRequest(userId.ToString()!, user.Role.ToString()));

        // Assert
        Assert.NotNull(response.Content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(ApiMessages.InvalidToken, await response.Content.ReadFromJsonAsync<string>());
    }

    [Theory]
    [ClassData(typeof(UserData))]
    public async Task Refresh_WithValidAndUnconfirmedUser_ReturnsBadRequestAndErrorMessage(TestUser user)
    {
        // Arrange
        var userId = await Factory.SeedUser(user);
        var loginResponse = await Client.PostAsJsonAsync(ApiPaths.Login, new LoginRequest(user.Email, user.Password));

        // read cookie
        var userCookies = loginResponse.Headers.GetValues("Set-Cookie").ToList();
        var refreshTokenType = user.Role is Role.Admin
            ? CookieManager.AdminRefreshCookieName
            : CookieManager.CustomerRefreshCookieName;
        var userRefreshToken = userCookies.FirstOrDefault(c => c.Contains(refreshTokenType));

        // Act
        Client.DefaultRequestHeaders.Add("Cookie", userRefreshToken);
        var response = await Client.PostAsJsonAsync(ApiPaths.Refresh,
            new RefreshOrLogoutRequest(userId.ToString()!, user.Role.ToString()));

        // Assert
        Assert.NotNull(response.Content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(ApiMessages.InvalidToken, await response.Content.ReadFromJsonAsync<string>());
    }

    [Theory]
    [ClassData(typeof(UserData))]
    public async Task Refresh_WithInvalidUser_ReturnsErrorMessage(TestUser user)
    {
        // Arrange
        await Factory.SeedUser(user);
        var loginResponse = await Client.PostAsJsonAsync(ApiPaths.Login, new LoginRequest(user.Email, user.Password));

        // read cookie
        var userCookies = loginResponse.Headers.GetValues("Set-Cookie").ToList();
        var refreshTokenType = user.Role is Role.Admin
            ? CookieManager.AdminRefreshCookieName
            : CookieManager.CustomerRefreshCookieName;
        var userRefreshToken = userCookies.FirstOrDefault(c => c.Contains(refreshTokenType));

        // Act
        Client.DefaultRequestHeaders.Add("Cookie", userRefreshToken);
        var response = await Client.PostAsJsonAsync(ApiPaths.Refresh,
            new RefreshOrLogoutRequest(Guid.NewGuid().ToString(), user.Role.ToString()));

        // Assert
        Assert.NotNull(response.Content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(ApiMessages.InvalidToken, await response.Content.ReadFromJsonAsync<string>());
    }
}