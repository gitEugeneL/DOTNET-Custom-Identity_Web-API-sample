using System.Security.Claims;

namespace Api.Tools;

public static class JwtReader
{
    public static Guid? ReadUserId(HttpContext httpContext)
    {
        var result = Guid.TryParse(
            httpContext
                .User
                .FindFirstValue(ClaimTypes.NameIdentifier),
            out var userId
        );
        return result
            ? userId
            : null;
    }

    public static string? ReadUserEmail(HttpContext httpContext)
    {
        return httpContext
            .User
            .FindFirstValue(ClaimTypes.Email);
    }

    public static string? ReadUserRole(HttpContext httpContext)
    {
        return httpContext
            .User
            .FindFirstValue(ClaimTypes.Role);
    }
}