namespace Api.Constants;

public static class ApiMessages
{
    public static string ConflictResultMessage(string type, string value)
    {
        return $"{type}: '{value}' already exists";
    }

    public static string NotFoundResultMessage(string type, string id)
    {
        return $"{type} with id: '{id}' not found";
    }

    public static string InvalidAuthMessage()
    {
        return "login or password is incorrect or account is locked or not confirmed";
    }
}