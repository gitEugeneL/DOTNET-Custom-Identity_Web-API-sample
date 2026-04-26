using System.Net;
using System.Net.Http.Json;
using Api.Constants;
using Api.Domain.Entities;
using Api.Features.Registration;
using Api.IntegrationTests.TestData;

namespace Api.IntegrationTests.Features;

public class RegistrationTests(ApiWebApplicationFactory factory)
    : IntegrationTestBase(factory), IClassFixture<ApiWebApplicationFactory>
{
    [Theory]
    [ClassData(typeof(UserData))]
    public async Task Registration_WithValidData_ReturnsCreatedResultWithUserId(TestUser user)
    {
        // Arrange
        var request = new RegistrationRequest(user.Email, user.Password, user.Password);

        // Act
        var response = await Client.PostAsJsonAsync(ApiPaths.Registration, request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(Guid.Empty, result);
    }

    [Theory]
    [ClassData(typeof(UserData))]
    public async Task Registration_WithConflictEmail_ReturnsConflictResultWithConflictMessage(TestUser user)
    {
        // Arrange
        await Factory.SeedUser(user);
        var request = new RegistrationRequest(user.Email, user.Password, user.Password);

        // Act
        var response = await Client.PostAsJsonAsync(ApiPaths.Registration, request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<string>();
        Assert.Equal(ApiMessages.ConflictResultMessage(nameof(User), user.Email), result);
    }
}