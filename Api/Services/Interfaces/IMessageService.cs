namespace Api.Services.Interfaces;

public interface IMessageService
{
    public Task<bool> SendMessageAsync(string to, string subject, string body, DateTime expires);
}