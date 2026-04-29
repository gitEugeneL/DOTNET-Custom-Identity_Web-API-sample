using Api.Domain.Enums;
using Api.Features.Common;
using Api.Features.Refresh;

namespace Api.Tests.Features;

public class RefreshRequestTests
{
    private readonly Validator _refreshRequestValidator = new();

    public static IEnumerable<object[]> ValidData()
    {
        yield return [Guid.NewGuid().ToString(), nameof(Role.Admin)];
        yield return [Guid.NewGuid().ToString(), nameof(Role.Customer)];
        yield return [Guid.NewGuid().ToString(), "CUSTOMER"];
        yield return [Guid.NewGuid().ToString(), "customer"];
        yield return [Guid.NewGuid().ToString(), "ADMIN"];
        yield return [Guid.NewGuid().ToString(), "admin"];
    }

    public static IEnumerable<object[]> InvalidData()
    {
        yield return ["", nameof(Role.Admin)]; // Empty userId
        yield return [Guid.NewGuid().ToString(), ""]; // Empty Role
    }

    [Theory]
    [MemberData(nameof(ValidData))]
    public async Task ValidLogoutRequest_PassesValidation(string userId, string role)
    {
        // Arrange
        var model = new RefreshOrLogoutRequest(userId, role);

        // Act
        var result = await _refreshRequestValidator.ValidateAsync(model);

        // Assert
        Assert.True(result.IsValid);
    }


    [Theory]
    [MemberData(nameof(InvalidData))]
    public async Task InvalidLogoutRequest_FailsValidation(string userId, string role)
    {
        // Arrange
        var model = new RefreshOrLogoutRequest(userId, role);

        // Act
        var result = await _refreshRequestValidator.ValidateAsync(model);

        // Assert
        Assert.False(result.IsValid);
    }
}