using Api.Domain.Entities;

namespace Api.Services.Interfaces;

public interface ILockoutService
{
    bool IsLoginLocked(User user);

    bool IsConfirmLocked(User user);

    void ResetLoginLockout(User user);

    void ResetConfirmLockout(User user);

    bool IsLoginAttemptLimitExceeded(User user);

    bool IsConfirmAttemptLimitExceeded(User user);

    bool IsGenerateCodeAttemptLimitExceeded(User user);
}