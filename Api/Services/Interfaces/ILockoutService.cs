using Api.Domain.Entities;

namespace Api.Services.Interfaces;

public interface ILockoutService
{
    void ProcessForGenerateCode(User user);

    void ProcessForConfirm(User user, bool isCodeValid);

    void ProcessForLogin(User user, bool isPasswordValid);
}