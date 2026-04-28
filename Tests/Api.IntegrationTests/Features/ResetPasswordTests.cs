using System.Net;
using System.Net.Http.Json;
using Api.Constants;
using Api.Features.ChangePassword;
using Api.Features.GenerateCode;
using Api.IntegrationTests.FakeServices;
using Api.IntegrationTests.TestData;

namespace Api.IntegrationTests.Features;

public class ResetPasswordTests(ApiWebApplicationFactory factory)
    : IntegrationTestBase(factory), IClassFixture<ApiWebApplicationFactory>
{
    [Theory]
    [ClassData(typeof(UserData))]
    public async Task ResetPassword_WithValidUserAndValidCode_ReturnsOKAndResetResult(TestUser user)
    {
        // Arrange
        var codeLength = Factory.GetRequiredConfig<int>("Authentication:Code.Length");

        // seed user
        user = user with { EmailConfirmed = true };
        await Factory.SeedUser(user);

        var newPassword = user.Password + user.Email + user.Password;

        // generate code
        await Client.PostAsJsonAsync(ApiPaths.GenerateCode, new GenerateCodeRequest(user.Email));

        // confirmation code (fake code)
        var validCode = new string(Enumerable.Repeat(FakeConfirmationService.ValidFakeCode, codeLength).ToArray());

        var request = new ChangePasswordRequest(user.Email, validCode, newPassword, newPassword);

        // Act
        var response = await Client.PostAsJsonAsync(ApiPaths.ChangePassword, request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ChangePasswordResponse>();

        Assert.NotNull(result);
        Assert.Equal(result.Email, user.Email, StringComparer.OrdinalIgnoreCase);
        Assert.True(result.IsPasswordChanged);
    }

    [Theory]
    [ClassData(typeof(UserData))]
    public async Task ResetPassword_WithValidAndUnconfirmedUserAndValidCode_ReturnsBadRequestAndErrorMessage(
        TestUser user)
    {
        // Arrange
        var codeLength = Factory.GetRequiredConfig<int>("Authentication:Code.Length");

        // seed user
        await Factory.SeedUser(user);

        var newPassword = user.Password + user.Email + user.Password;

        // generate code
        await Client.PostAsJsonAsync(ApiPaths.GenerateCode, new GenerateCodeRequest(user.Email));

        // confirmation code (fake code)
        var validCode = new string(Enumerable.Repeat(FakeConfirmationService.ValidFakeCode, codeLength).ToArray());

        var request = new ChangePasswordRequest(user.Email, validCode, newPassword, newPassword);

        // Act
        var response = await Client.PostAsJsonAsync(ApiPaths.ChangePassword, request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<string>();

        Assert.NotNull(result);
        Assert.Equal(ApiMessages.InvalidConfirm, result);
    }

    [Theory]
    [ClassData(typeof(UserData))]
    public async Task ResetPassword_WithValidUserAndInvalidCode_ReturnsBadRequestAndErrorMessage(TestUser user)
    {
        // Arrange
        var codeLength = Factory.GetRequiredConfig<int>("Authentication:Code.Length");

        user = user with { EmailConfirmed = true };
        await Factory.SeedUser(user);

        var newPassword = user.Password + user.Email + user.Password;

        // generate code
        await Client.PostAsJsonAsync(ApiPaths.GenerateCode, new GenerateCodeRequest(user.Email));

        // confirmation code (fake code)
        var invalidCode = new string(Enumerable.Repeat(FakeConfirmationService.InvalidFakeCode, codeLength).ToArray());

        var request = new ChangePasswordRequest(user.Email, invalidCode, newPassword, newPassword);

        // Act
        var response = await Client.PostAsJsonAsync(ApiPaths.ChangePassword, request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<string>();

        Assert.NotNull(result);
        Assert.Equal(ApiMessages.InvalidConfirm, result);
    }

    [Theory]
    [ClassData(typeof(UserData))]
    public async Task ResetPassword_WithValidUserAndUngeneratedCode_ReturnsBadRequestAndErrorMessage(TestUser user)
    {
        // Arrange
        var codeLength = Factory.GetRequiredConfig<int>("Authentication:Code.Length");

        user = user with { EmailConfirmed = true };
        await Factory.SeedUser(user);

        var newPassword = user.Password + user.Email + user.Password;

        // confirmation code (fake code)
        var code = new string(Enumerable.Repeat(FakeConfirmationService.ValidFakeCode, codeLength).ToArray());

        var request = new ChangePasswordRequest(user.Email, code, newPassword, newPassword);

        // Act
        var response = await Client.PostAsJsonAsync(ApiPaths.ChangePassword, request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<string>();

        Assert.NotNull(result);
        Assert.Equal(ApiMessages.InvalidConfirm, result);
    }

    [Theory]
    [ClassData(typeof(UserData))]
    public async Task ResetPassword_WithInvalidUser_ReturnsBadRequestAndErrorMessage(TestUser user)
    {
        // Arrange
        var codeLength = Factory.GetRequiredConfig<int>("Authentication:Code.Length");

        var newPassword = user.Password + user.Email + user.Password;

        // generate code
        await Client.PostAsJsonAsync(ApiPaths.GenerateCode, new GenerateCodeRequest(user.Email));

        // confirmation code (fake code)
        var invalidCode = new string(Enumerable.Repeat(FakeConfirmationService.InvalidFakeCode, codeLength).ToArray());

        var request = new ChangePasswordRequest(user.Email, invalidCode, newPassword, newPassword);

        // Act
        var response = await Client.PostAsJsonAsync(ApiPaths.ChangePassword, request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<string>();

        Assert.NotNull(result);
        Assert.Equal(ApiMessages.InvalidConfirm, result);
    }

    [Theory]
    [ClassData(typeof(UserData))]
    public async Task ResetPassword_WithLockoutUserAndValidCode_ReturnsBadRequestAndErrorMessage(TestUser user)
    {
        // Arrange
        var codeLength = Factory.GetRequiredConfig<int>("Authentication:Code.Length");
        var codeMaxAttempts = Factory.GetRequiredConfig<int>("Authentication:Code.MaxAttempts");

        // seed user
        await Factory.SeedUser(user);

        var newPassword = user.Password + user.Email + user.Password;

        // generate code (multiple attempts)
        for (var i = 0; i <= codeMaxAttempts; i++)
            await Client.PostAsJsonAsync(ApiPaths.GenerateCode, new GenerateCodeRequest(user.Email));

        // confirmation code (fake code)
        var validCode = new string(Enumerable.Repeat(FakeConfirmationService.ValidFakeCode, codeLength).ToArray());

        var request = new ChangePasswordRequest(user.Email, validCode, newPassword, newPassword);

        // Act
        var response = await Client.PostAsJsonAsync(ApiPaths.ChangePassword, request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<string>();

        Assert.NotNull(result);
        Assert.Equal(ApiMessages.InvalidConfirm, result);
    }
}