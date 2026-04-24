using Api.Domain.Entities;
using Api.Services.Interfaces;

namespace Api.Services;

public class LockoutService(IConfiguration configuration) : ILockoutService
{
    public void ProcessForGenerateCode(User user)
    {
        var now = DateTime.UtcNow;

        var maxAttempts = GetValue("Authentication:Code.MaxAttempts");
        var lockoutMinutes = GetValue("Authentication:ConfirmLockout.Lifetime.Minutes");

        ResetConfirmLockIfExpired(user, now);

        user.GenerateCodeCount++;

        if (user.GenerateCodeCount < maxAttempts)
            return;

        user.ConfirmLocked = true;
        user.ConfirmLockExpires = now.AddMinutes(lockoutMinutes);
    }

    public void ProcessForLogin(User user, bool isPasswordValid)
    {
        var now = DateTime.UtcNow;

        var maxAttempts = GetValue("Authentication:LoginLockout.MaxAttempts");
        var lockoutMinutes = GetValue("Authentication:LoginLockout.Lifetime.Minutes");

        ResetLoginLockIfExpired(user, now);

        user.LoginFailedCount = isPasswordValid
            ? 0
            : user.LoginFailedCount + 1;

        if (user.LoginFailedCount < maxAttempts)
            return;

        user.LoginLocked = true;
        user.LoginLockExpires = now.AddMinutes(lockoutMinutes);
    }

    public void ProcessForConfirm(User user, bool isCodeValid)
    {
        var now = DateTime.UtcNow;

        var maxAttempts = GetValue("Authentication:ConfirmLockout.MaxAttempts");
        var lockoutMinutes = GetValue("Authentication:ConfirmLockout.Lifetime.Minutes");

        ResetConfirmLockIfExpired(user, now);

        if (isCodeValid)
        {
            user.GenerateCodeCount = 0;
            user.ConfirmFailedCount = 0;
            user.EmailConfirmed = true;
            return;
        }

        user.ConfirmFailedCount++;

        if (user.ConfirmFailedCount < maxAttempts)
            return;

        user.ConfirmLocked = true;
        user.ConfirmLockExpires = now.AddMinutes(lockoutMinutes);
    }

    private int GetValue(string key)
    {
        return int.Parse(configuration[key]
                         ?? throw new ApplicationException($"{key} not found in configuration"));
    }

    private static void ResetLoginLockIfExpired(User user, DateTime now)
    {
        if (user is not { LoginLocked: true, LoginLockExpires: not null } || !(user.LoginLockExpires < now))
            return;

        user.LoginFailedCount = 0;
        user.LoginLocked = false;
        user.LoginLockExpires = null;
    }

    private static void ResetConfirmLockIfExpired(User user, DateTime now)
    {
        if (!user.ConfirmLocked || user.ConfirmLockExpires is null || !(user.ConfirmLockExpires < now))
            return;

        user.GenerateCodeCount = 0;
        user.ConfirmFailedCount = 0;
        user.ConfirmLocked = false;
        user.ConfirmLockExpires = null;
    }
}