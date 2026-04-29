using Api.Services;

namespace Api.Tests.Services;

public class MessageServiceTests
{
    private readonly string _filePath;

    public MessageServiceTests()
    {
        _filePath = Path.Combine(Directory.GetCurrentDirectory(), MessageService.FileName);

        if (File.Exists(_filePath))
            File.Delete(_filePath);
    }

    [Theory]
    [InlineData("test@example.com", "Test Subject", "Test Body", 10)]
    [InlineData("admin@domain.com", "Important Notice", "This is an important message", 30)]
    [InlineData("user@test.org", "Welcome", "Welcome to our service!", 60)]
    [InlineData("contact@company.net", "Reminder", "Don't forget about the meeting", 5)]
    [InlineData("support@service.io", "Confirmation", "Your request has been processed", 15)]
    public async Task SendMessage_WriteToFile_ReturnsTrue(string to, string subject, string body, int minutes)
    {
        // Arrange
        var service = new MessageService();
        var expires = DateTime.UtcNow.AddMinutes(minutes);

        // Act
        var result = await service.SendMessageAsync(to, subject, body, expires);

        // Assert
        Assert.True(result);
        Assert.True(File.Exists(_filePath));

        var resultContent = await File.ReadAllTextAsync(_filePath);

        Assert.Contains($"To: {to}", resultContent);
        Assert.Contains($"Subject: {subject}", resultContent);
        Assert.Contains($"Body: {body}", resultContent);
        Assert.Contains($"Expires: {expires}", resultContent);
    }
}