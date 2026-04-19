namespace Api.Constants;

public static class ApiMessages
{
    public const string InvalidAuth = "login or password is incorrect or account is locked or not confirmed";

    public const string InvalidToken = "user not found or token is invalid";

    public const string InvalidConfirm = "user not found or account is locked";

    public static string ConflictResultMessage(string type, string value)
    {
        return $"{type}: '{value}' already exists";
    }

    public static string NotFoundResultMessage(string type, string id)
    {
        return $"{type} with id: '{id}' not found";
    }

    public static string InvalidIdFormatResultMessage(string value)
    {
        return $"parameter: '{value}' is invalid format";
    }
}