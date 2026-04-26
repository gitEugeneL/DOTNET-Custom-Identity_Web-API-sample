using Api.Constants;

namespace Api.Tools;

public static class CookieManager
{
    public const string AdminRefreshCookieName = "refreshTokenAdmin";
    public const string CustomerRefreshCookieName = "refreshTokenCustomer";

    private static string GetRefreshCookieName(string roleName)
    {
        return roleName == AuthPolicies.Admin ? AdminRefreshCookieName : CustomerRefreshCookieName;
    }

    public static void SetCookie(HttpContext context, string refreshToken, DateTime expires, string clientRole)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = expires
        };
        context.Response.Cookies.Append(GetRefreshCookieName(clientRole), refreshToken, cookieOptions);
    }

    public static string? ReadCookie(HttpContext context, string clientRole)
    {
        context.Request.Cookies.TryGetValue(GetRefreshCookieName(clientRole), out var refreshToken);
        return refreshToken;
    }

    public static void RemoveCookie(HttpContext context, string clientRole)
    {
        context.Response.Cookies.Delete(GetRefreshCookieName(clientRole));
    }
}