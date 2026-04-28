using Api.Constants;
using Api.Extensions.Interfaces;
using Api.Tools;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.UserInfo;

public class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiPaths.Info, HandleAsync)
            .RequireAuthorization(AuthPolicies.BasePolicy)
            .RequireRateLimiting(RateLimitPolicies.BasePolicy);
    }

    private static async Task<Results<UnauthorizedHttpResult, Ok<UserInfoResponse>>> HandleAsync(
        HttpContext httpContext,
        Data data,
        CancellationToken ct)
    {
        var userId = JwtReader.ReadUserId(httpContext);
        if (userId is null)
            return TypedResults.Unauthorized();

        var user = await data.GetUser(userId.Value, ct);
        return user is not null
            ? TypedResults.Ok(new UserInfoResponse(
                user.Email,
                user.ConfirmLocked,
                user.EmailConfirmed,
                user.ConfirmLockExpires))
            : TypedResults.Unauthorized();
    }
}