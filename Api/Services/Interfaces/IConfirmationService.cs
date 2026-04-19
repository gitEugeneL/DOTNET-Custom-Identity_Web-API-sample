using Api.Domain.Entities;

namespace Api.Services.Interfaces;

public interface IConfirmationService
{
    (string code, DateTime expires) GenerateCode();

    bool IsCodeValid(User user, string code);
}