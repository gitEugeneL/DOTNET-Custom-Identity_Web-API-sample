using Api.Domain.Entities;
using Api.Domain.Enums;
using Api.Services;
using Microsoft.Extensions.Configuration;

namespace Api.Tests.Services;

public class LockoutServiceTests
{
    private const int CodeMaxAttempts = 5;

    private const int ConfirmMaxAttempts = 5;

    private const int LoginMaxAttempts = 5;

    private const int ConfirmLockoutLifetimeMinutes = 10;

    private const int LoginLockoutLifetimeMinutes = 10;


    private readonly IConfiguration _configuration;

    public LockoutServiceTests()
    {
        var configurationSettings = new Dictionary<string, string?>
        {
            { "Authentication:Code.MaxAttempts", CodeMaxAttempts.ToString() },
            { "Authentication:ConfirmLockout.Lifetime.Minutes", ConfirmLockoutLifetimeMinutes.ToString() },
            { "Authentication:LoginLockout.MaxAttempts", LoginMaxAttempts.ToString() },
            { "Authentication:LoginLockout.Lifetime.Minutes", LoginLockoutLifetimeMinutes.ToString() },
            { "Authentication:ConfirmLockout.MaxAttempts", ConfirmMaxAttempts.ToString() }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationSettings)
            .Build();
    }

    private static User CreateTestUser(string email, Role role)
    {
        return new User
        {
            GenerateCodeCount = 0,
            LoginFailedCount = 0,
            ConfirmFailedCount = 0,
            Email = email,
            Role = role
        };
    }

    [Theory]
    [InlineData("admin@example.com", Role.Admin)]
    [InlineData("dev@dev.com", Role.Customer)]
    [InlineData("manager@company.org", Role.Customer)]
    public void ProcessForGenerateCode_ShouldLock_WhenMaxAttemptsReached(string email, Role role)
    {
        // Arrange
        var service = new LockoutService(_configuration);
        var user = CreateTestUser(email, role);

        user.GenerateCodeCount = CodeMaxAttempts - 1;

        // Act
        service.ProcessForGenerateCode(user);

        // Assert
        Assert.True(user.ConfirmLocked);
        Assert.NotNull(user.ConfirmLockExpires);
    }

    [Theory]
    [InlineData("admin@example.com", Role.Admin)]
    [InlineData("dev@dev.com", Role.Customer)]
    [InlineData("manager@company.org", Role.Customer)]
    public void ProcessForGenerateCode_ShouldNotLock_BelowLimit(string email, Role role)
    {
        // Arrange
        var service = new LockoutService(_configuration);
        var user = CreateTestUser(email, role);

        // Act
        service.ProcessForGenerateCode(user);

        // Assert
        Assert.False(user.ConfirmLocked);
        Assert.Equal(user.ConfirmFailedCount + 1, user.GenerateCodeCount);
    }

    [Theory]
    [InlineData("admin@example.com", Role.Admin)]
    [InlineData("dev@dev.com", Role.Customer)]
    [InlineData("manager@company.org", Role.Customer)]
    public void ProcessForLogin_ShouldReset_OnSuccess(string email, Role role)
    {
        // Arrange
        var service = new LockoutService(_configuration);
        var user = CreateTestUser(email, role);

        user.LoginFailedCount = LoginMaxAttempts - 1;

        // Act
        service.ProcessForLogin(user, true);

        // Assert
        Assert.Equal(0, user.LoginFailedCount);
        Assert.False(user.LoginLocked);
    }

    [Theory]
    [InlineData("admin@example.com", Role.Admin)]
    [InlineData("dev@dev.com", Role.Customer)]
    [InlineData("manager@company.org", Role.Customer)]
    public void ProcessForLogin_ShouldLock_WhenMaxAttemptsReached(string email, Role role)
    {
        // Arrange
        var service = new LockoutService(_configuration);
        var user = CreateTestUser(email, role);

        user.LoginFailedCount = LoginMaxAttempts - 1;

        service.ProcessForLogin(user, false);

        // Assert
        Assert.True(user.LoginLocked);
        Assert.NotNull(user.LoginLockExpires);
    }

    [Theory]
    [InlineData("admin@example.com", Role.Admin)]
    [InlineData("dev@dev.com", Role.Customer)]
    [InlineData("manager@company.org", Role.Customer)]
    public void ProcessForConfirm_ShouldConfirm_OnValidCode(string email, Role role)
    {
        // Arrange
        var service = new LockoutService(_configuration);
        var user = CreateTestUser(email, role);

        user.ConfirmFailedCount = ConfirmMaxAttempts - 1;
        user.GenerateCodeCount = CodeMaxAttempts - 1;

        // Act
        service.ProcessForConfirm(user, true);

        // Assert
        Assert.True(user.EmailConfirmed);
        Assert.Equal(0, user.GenerateCodeCount);
        Assert.Equal(0, user.ConfirmFailedCount);
    }

    [Theory]
    [InlineData("admin@example.com", Role.Admin)]
    [InlineData("dev@dev.com", Role.Customer)]
    [InlineData("manager@company.org", Role.Customer)]
    public void ProcessForConfirm_ShouldLock_WhenMaxAttemptsReached(string email, Role role)
    {
        // Arrange
        var service = new LockoutService(_configuration);
        var user = CreateTestUser(email, role);

        user.ConfirmFailedCount = ConfirmMaxAttempts - 1;

        // Act
        service.ProcessForConfirm(user, false);

        // Assert
        Assert.True(user.ConfirmLocked);
        Assert.NotNull(user.ConfirmLockExpires);
    }

    [Theory]
    [InlineData("admin@example.com", Role.Admin)]
    [InlineData("dev@dev.com", Role.Customer)]
    [InlineData("manager@company.org", Role.Customer)]
    public void ResetLoginLockIfExpired_ShouldReset_WhenExpired(string email, Role role)
    {
        // Arrange
        var service = new LockoutService(_configuration);
        var user = CreateTestUser(email, role);

        user.LoginLocked = true;
        user.LoginFailedCount = LoginMaxAttempts;
        user.LoginLockExpires = DateTime.UtcNow.AddMinutes(-1);

        // Act
        service.ProcessForLogin(user, false);

        // Assert
        Assert.False(user.LoginLocked);
        Assert.Equal(1, user.LoginFailedCount);
    }

    [Theory]
    [InlineData("admin@example.com", Role.Admin)]
    [InlineData("dev@dev.com", Role.Customer)]
    [InlineData("manager@company.org", Role.Customer)]
    public void ResetConfirmLockIfExpired_ShouldReset_WhenExpired(string email, Role role)
    {
        // Arrange
        var service = new LockoutService(_configuration);
        var user = CreateTestUser(email, role);

        user.ConfirmLocked = true;
        user.GenerateCodeCount = CodeMaxAttempts;
        user.ConfirmFailedCount = ConfirmMaxAttempts;
        user.ConfirmLockExpires = DateTime.UtcNow.AddMinutes(-1);

        // Act
        service.ProcessForConfirm(user, false);

        // Assert
        Assert.False(user.ConfirmLocked);
        Assert.Equal(1, user.ConfirmFailedCount);
    }
}