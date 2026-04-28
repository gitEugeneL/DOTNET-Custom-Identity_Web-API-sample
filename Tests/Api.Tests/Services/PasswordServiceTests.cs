using Api.Services;

namespace Api.Tests.Services;

public class PasswordServiceTests
{
    [Theory]
    [InlineData("StrongPassword")]
    [InlineData("password!@3!@#")]
    public void CreatePasswordHash_WithValidPassword_GeneratesHashAndSalt(string password)
    {
        // Arrange
        var passwordManager = new PasswordService();

        // Act
        passwordManager.CreatePasswordHash(password, out var hash, out var salt);

        // Assert
        Assert.NotNull(hash);
        Assert.NotNull(salt);
    }

    [Theory]
    [InlineData("StrongPassword")]
    [InlineData("password!@3!@#")]
    [InlineData("Psw!@3!232-3")]
    [InlineData("123!@#@#12asdASD")]
    public void VerifyPasswordHash_WithCorrectPassword_ReturnsTrue(string password)
    {
        // Arrange
        var passwordManager = new PasswordService();
        passwordManager.CreatePasswordHash(password, out var hash, out var salt);

        // Act
        var result = passwordManager.VerifyPasswordHash(password, hash, salt);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("StrongPassword")]
    [InlineData("password!@3!@#")]
    [InlineData("Psw!@3!232-3")]
    [InlineData("123!@#@#12asdASD")]
    public void VerifyPasswordHash_WithIncorrectPassword_ReturnsTrue(string password)
    {
        // Arrange
        var passwordManager = new PasswordService();
        passwordManager.CreatePasswordHash("invalid-password", out var hash, out var salt);

        // Act
        var result = passwordManager.VerifyPasswordHash(password, hash, salt);

        // Assert
        Assert.False(result);
    }
}