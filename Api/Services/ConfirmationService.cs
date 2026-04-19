using Api.Domain.Entities;
using Api.Services.Interfaces;

namespace Api.Services;

public class ConfirmationService(IConfiguration configuration) : IConfirmationService
{
    public (string code, DateTime expires) GenerateCode()
    {
        var codeLength = int.Parse(configuration["Authentication:Code.Length"]!);
        var expires = DateTime.UtcNow.AddMinutes(int.Parse(configuration["Authentication:Code.Lifetime.Minutes"]!));
        var code = Random.Shared.Next((int)Math.Pow(10, codeLength - 1), (int)Math.Pow(10, codeLength)).ToString();
        return (code, expires);
    }

    public bool IsCodeValid(User user, string code)
    {
        throw new NotImplementedException();


        // user.ConfirmationCode

        // return user.ConfirmationCode is not null
        // && user.ConfirmationCode.Code == code && user.ConfirmationCode.Expires >= DateTime.UtcNow;
    }
}