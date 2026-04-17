namespace Api.Constants;

public static class ApiPaths
{
    private const string Base = "/api/auth";

    public const string Registration = Base + "/registration";
    public const string Login = Base + "/login";
    public const string Refresh = Base + "/refresh";


    // public const string Logout            = Base + "/logout";
    // public const string EmailConfirmation = Base + "/email-confirmation";
    // public const string ForgotPassword    = Base + "/forgot-password";
    // public const string ResetPassword     = Base + "/reset-password";
}