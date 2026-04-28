using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using Api.Constants;
using Api.Domain.Enums;
using Api.Features.Common;
using Api.Features.Login;
using Api.IntegrationTests.TestData;
using Api.Tools;

namespace Api.IntegrationTests.Features;

public class LogoutTests(ApiWebApplicationFactory factory)
    : IntegrationTestBase(factory), IClassFixture<ApiWebApplicationFactory>
{
    [Theory]
    [ClassData(typeof(UserData))]
    public async Task Logout_WithValidUserAndValidRefreshToken_ReturnsNoContentAndRemoveCookie(TestUser user)
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
        var response = await Client.PostAsJsonAsync(
            ApiPaths.Logout,
            new RefreshOrLogoutRequest(userId.ToString()!, user.Role.ToString())
        );

        // Assert
        Assert.NotNull(response.Content);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var resultCookies = response.Headers.GetValues("Set-Cookie").ToList();
        Assert.DoesNotContain(userRefreshToken, resultCookies);
    }

    [Theory]
    [ClassData(typeof(UserData))]
    public async Task Logout_WithValidUserAndInvalidRefreshToken_ReturnsBadRequestAndErrorMessage(TestUser user)
    {
        // Arrange
        var userId = await Factory.SeedUser(user);

        var invalidRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(256));

        // Act
        Client.DefaultRequestHeaders.Add("Cookie", invalidRefreshToken);
        var response = await Client.PostAsJsonAsync(
            ApiPaths.Logout,
            new RefreshOrLogoutRequest(userId.ToString()!, user.Role.ToString())
        );

        // Assert
        Assert.NotNull(response.Content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<string>();

        Assert.NotNull(result);
        Assert.Equal(ApiMessages.InvalidToken, result);
    }

    [Theory]
    [ClassData(typeof(UserData))]
    public async Task Logout_WithInvalidUser_ReturnsBadRequestAndErrorMessage(TestUser user)
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
        var response = await Client.PostAsJsonAsync(
            ApiPaths.Logout,
            new RefreshOrLogoutRequest(Guid.NewGuid().ToString(), user.Role.ToString())
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(ApiMessages.InvalidToken, await response.Content.ReadFromJsonAsync<string>());
    }
}