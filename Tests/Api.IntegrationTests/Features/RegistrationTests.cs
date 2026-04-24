using System.Net;
using System.Net.Http.Json;
using Api.Constants;
using Api.Domain.Entities;
using Api.Features.Registration;

namespace Api.IntegrationTests.Features;

public class RegistrationTests(ApiWebApplicationFactory factory) : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Theory]
    [InlineData("mailt@mail.test", "strongPwd!1", "strongPwd!1")]
    [InlineData("mail1@mail.test", "myPassword12@", "myPassword12@")]
    public async Task Registration_WithValidData_ReturnsCreatedResultWithUserId
        (string email, string password, string confirmPassword)
    {
        // Arrange
        var request = new RegistrationRequest(email, password, confirmPassword);

        // Act
        var response = await _client.PostAsJsonAsync(ApiPaths.Registration, request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(Guid.Empty, result);
    }

    [Theory]
    [InlineData("mailt@mail.test", "strongPwd!1", "strongPwd!1")]
    [InlineData("mail1@mail.test", "myPassword12@", "myPassword12@")]
    public async Task Registration_WithConflictEmail_ReturnsConflictResultWithConflictMessage
        (string email, string password, string confirmPassword)
    {
        // Arrange
        var request = new RegistrationRequest(email, password, confirmPassword);

        // Act
        var response = new HttpResponseMessage();
        for (var i = 0; i < 2; i++)
            response = await _client.PostAsJsonAsync(ApiPaths.Registration, request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<string>();
        Assert.Equal(ApiMessages.ConflictResultMessage(nameof(User), email), result);
    }
}