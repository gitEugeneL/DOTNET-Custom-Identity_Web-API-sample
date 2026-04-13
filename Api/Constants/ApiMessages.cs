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

    public static string InvalidIdFormatResultMessage(string id)
    {
        return $"id: '{id}' is invalid format";
    }
}