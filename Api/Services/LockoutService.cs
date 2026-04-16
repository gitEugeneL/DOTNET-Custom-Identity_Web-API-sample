using Api.Domain.Entities;
using Api.Services.Interfaces;

namespace Api.Services;

public class LockoutService(IConfiguration configuration) : ILockoutService
{
    public bool IsLoginLocked(User user)
    {
        return IsLocked(user, user.LoginLocked, user.LoginLockExpires, ResetLoginLockout);
    }

    public bool IsConfirmLocked(User user)
    {
        return IsLocked(user, user.ConfirmLocked, user.ConfirmLockExpires, ResetConfirmLockout);
    }

    public bool IsLoginAttemptLimitExceeded(User user)
    {
        return IsAttemptLimitExceeded(user, "LoginLockout", user.LoginFailedCount, SetLoginLockout);
    }

    public bool IsConfirmAttemptLimitExceeded(User user)
    {
        return IsAttemptLimitExceeded(user, "ConfirmLockout", user.ConfirmFailedCount, SetConfirmLockout);
    }

    public bool IsGenerateCodeAttemptLimitExceeded(User user)
    {
        return IsAttemptLimitExceeded(user, "Code", user.GenerateCodeCount, SetConfirmLockout);
    }

    public void ResetLoginLockout(User user)
    {
        user.LoginLocked = false;
        user.LoginLockExpires = null;
        user.LoginFailedCount = 0;
    }

    public void ResetConfirmLockout(User user)
    {
        user.ConfirmLocked = false;
        user.ConfirmLockExpires = null;
        user.ConfirmFailedCount = 0;
        user.GenerateCodeCount = 0;
    }

    private bool IsAttemptLimitExceeded(User user, string attemptType, int failedCount,
        Action<User, int> setLockoutAction)
    {
        var maxAttempts = int.Parse(configuration[$"Authentication:{attemptType}.MaxAttempts"]!);
        var minutes = int.Parse(configuration[$"Authentication:{attemptType}.Lifetime.Minutes"]!);

        if (failedCount <= maxAttempts)
            return false;

        setLockoutAction(user, minutes);
        return true;
    }

    private static void SetLoginLockout(User user, int minutes)
    {
        user.LoginLocked = true;
        user.LoginLockExpires = DateTime.UtcNow.AddMinutes(minutes);
    }

    private static void SetConfirmLockout(User user, int minutes)
    {
        user.ConfirmLocked = true;
        user.ConfirmLockExpires = DateTime.UtcNow.AddMinutes(minutes);
    }

    private static bool IsLocked(User user, bool isLocked, DateTime? lockExpires, Action<User> resetLockout)
    {
        switch (isLocked)
        {
            case true when lockExpires >= DateTime.UtcNow:
                return true;

            case true when lockExpires < DateTime.UtcNow:
                resetLockout(user);
                break;
        }

        return false;
    }
}