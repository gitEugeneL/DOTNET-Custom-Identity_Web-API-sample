using Api.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Api.IntegrationTests.FakeServices;

public class FakeConfirmationService(IConfiguration configuration) : IConfirmationService
{
    public const char ValidFakeCode = '1';
    public const char InvalidFakeCode = '0';

    public (string code, DateTime expires) GenerateCode()
    {
        var codeLength = int.Parse(configuration["Authentication:Code.Length"] ??
                                   throw new ApplicationException("Code.Length not found in configuration"));

        var expires = DateTime.UtcNow.AddMinutes(
            int.Parse(configuration["Authentication:Code.Lifetime.Minutes"] ??
                      throw new ApplicationException("Code.Lifetime.Minutes not found in configuration")));

        // fake code generation
        var fakeCode = new string(Enumerable.Repeat(ValidFakeCode, codeLength).ToArray());

        return (fakeCode, expires);
    }
}