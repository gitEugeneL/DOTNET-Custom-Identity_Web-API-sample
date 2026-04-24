using Api.Services.Interfaces;

namespace Api.Services;

public class ConfirmationService(IConfiguration configuration) : IConfirmationService
{
    public (string code, DateTime expires) GenerateCode()
    {
        var codeLength = int.Parse(configuration["Authentication:Code.Length"] ??
                                   throw new ApplicationException("Code.Length not found in configuration"));

        var expires = DateTime.UtcNow.AddMinutes(
            int.Parse(configuration["Authentication:Code.Lifetime.Minutes"] ??
                      throw new ApplicationException("Code.Lifetime.Minutes not found in configuration")));

        var code = Random.Shared.Next((int)Math.Pow(10, codeLength - 1), (int)Math.Pow(10, codeLength)).ToString();
        return (code, expires);
    }
}