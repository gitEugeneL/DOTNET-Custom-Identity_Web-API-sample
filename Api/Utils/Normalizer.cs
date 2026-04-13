using System.Globalization;

namespace Api.Utils;

public static class Normalizer
{
    public static string NormalizeImportantString(string value)
    {
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(value.Trim().ToUpperInvariant());
    }
}