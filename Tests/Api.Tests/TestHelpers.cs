namespace Api.Tests;

public static class TestHelpers
{
    public const int CodeLength = 6;

    public const int CodeLifeTimeMinutes = 5;

    public const int CodeMaxAttempts = 5;

    public const int ConfirmMaxAttempts = 5;

    public const int LoginMaxAttempts = 5;

    public const int ConfirmLockoutLifetimeMinutes = 10;

    public const int LoginLockoutLifetimeMinutes = 10;

    public const int AccessTokenLifeTimeMinutes = 10;

    public const int RefreshTokenLifeTimeDays = 30;

    public const int RefreshTokenMaxCount = 5;

    public const string AccessTokenSecurityKey = "OnlyDevSecurityKey123456789_DONT_USE_DONT_USE_DONT_USE_DONT_USE_!!";

    public const string Issuer = "TestIssuer";

    public const string Audience = "TestAudience";

    public static string GenerateValidCode(int length)
    {
        var max = (int)Math.Pow(10, length) - 1;
        var value = Random.Shared.Next(0, max + 1);
        return value.ToString($"D{length}");
    }
}