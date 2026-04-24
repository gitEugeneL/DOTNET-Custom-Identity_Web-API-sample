using System.Net;
using System.Net.Http.Json;
using Api.Constants;
using Api.Features.Registration;

namespace Api.IntegrationTests.Features;

public class RegistrationTests(ApiWebApplicationFactory factory) : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Theory]
    [InlineData("mailt@mail.test", "strongPwd!1", "strongPwd!1")]
    [InlineData("mail1@mail.test", "myPassword12@", "myPassword12@")]
    public async Task Registration_WithValidBody_ReturnsUserUd
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
}