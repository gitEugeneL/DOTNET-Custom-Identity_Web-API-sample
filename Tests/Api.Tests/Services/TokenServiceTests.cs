using System.IdentityModel.Tokens.Jwt;
using Api.Domain.Entities;
using Api.Domain.Enums;
using Api.Services;
using Microsoft.Extensions.Configuration;

namespace Api.Tests.Services;

public class TokenServiceTests
{
    private const int AccessTokenLifeTimeMinutes = 10;
    private const int RefreshTokenLifeTimeDays = 30;
    private const int RefreshTokenMaxCount = 5;
    private const string AccessTokenSecurityKey = "OnlyDevSecurityKey123456789_DONT_USE_DONT_USE_DONT_USE_DONT_USE_!!";
    private const string Issuer = "TestIssuer";
    private const string Audience = "TestAudience";

    private readonly IConfiguration _configuration;

    public TokenServiceTests()
    {
        var configurationSettings = new Dictionary<string, string?>
        {
            { "Authentication:AccessToken.SecurityKey", AccessTokenSecurityKey },
            { "Authentication:AccessToken.Lifetime.Minutes", AccessTokenLifeTimeMinutes.ToString() },
            { "Authentication:RefreshToken.Lifetime.Days", RefreshTokenLifeTimeDays.ToString() },
            { "Authentication:RefreshToken.MaxCount", RefreshTokenMaxCount.ToString() },
            { "Authentication:Issuer", Issuer },
            { "Authentication:Audience", Audience }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationSettings)
            .Build();
    }

    [Theory]
    [InlineData("dev@Dev.com", Role.Customer, true)]
    [InlineData("customer@customer.com", Role.Customer, true)]
    [InlineData("test@customer.com", Role.Customer, false)]
    [InlineData("test@test.com", Role.Customer, false)]
    public void GenerateAccessToken_WithValidData_ReturnsValidAccessToken(string email, Role role, bool emailConfirmed)
    {
        // Arrange
        var tokenService = new TokenService(_configuration);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Role = role,
            EmailConfirmed = emailConfirmed
        };

        // Act
        var (token, expires) = tokenService.GenerateAccessToken(user);

        // Assert
        Assert.NotNull(token);
        var difference = Math.Abs((expires - DateTime.UtcNow.AddMinutes(AccessTokenLifeTimeMinutes)).TotalSeconds);
        Assert.True(difference < 3);

        var accessToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var tokenId = accessToken.Claims.First(c => c.Type == "nameid").Value;
        var tokenEmail = accessToken.Claims.First(c => c.Type == "email").Value;
        var tokenRole = accessToken.Claims.First(c => c.Type == "role").Value;
        var tokenEmailConfirmed = accessToken.Claims.First(c => c.Type == "isEmailConfirmed").Value;

        Assert.Equal(user.Id.ToString(), tokenId);
        Assert.Equal(user.Email, tokenEmail);
        Assert.Equal(user.Role.ToString(), tokenRole);
        Assert.Equal(user.EmailConfirmed.ToString(), tokenEmailConfirmed);


        Assert.Equal(Issuer, accessToken.Issuer);
        Assert.Equal(Audience, accessToken.Audiences.First());
    }

    [Theory]
    [InlineData("dev@Dev.com", Role.Customer, true)]
    [InlineData("customer@customer.com", Role.Customer, true)]
    [InlineData("test@customer.com", Role.Customer, false)]
    [InlineData("test@test.com", Role.Customer, false)]
    public void GenerateRefreshToken_WithValidData_ReturnsValidRefreshToken(string email, Role role,
        bool emailConfirmed)
    {
        // Arrange
        var tokenService = new TokenService(_configuration);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Role = role,
            EmailConfirmed = emailConfirmed
        };

        // Act
        var refreshToken = tokenService.GenerateRefreshToken(user);

        // Assert
        Assert.NotNull(refreshToken);
        Assert.Equal(user.Id, refreshToken.UserId);
        Assert.False(string.IsNullOrWhiteSpace(refreshToken.Token));
        Assert.Equal(265, Convert.FromBase64String(refreshToken.Token).Length);

        var expectedExpires = DateTime.UtcNow.AddDays(RefreshTokenLifeTimeDays);
        var diff = Math.Abs((refreshToken.Expires - expectedExpires).TotalSeconds);

        Assert.True(diff < 3);
    }
}