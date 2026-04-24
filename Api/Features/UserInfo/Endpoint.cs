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
            .RequireAuthorization(AuthPolicies.BasePolicy);
    }

    private static async Task<Results<UnauthorizedHttpResult, Ok<string>>> HandleAsync(
        HttpContext httpContext,
        CancellationToken ct)
    {
        var userId = JwtReader.ReadUserId(httpContext);
        var role = JwtReader.ReadUserRole(httpContext);

        return userId is null || role is null
            ? TypedResults.Unauthorized()
            : TypedResults.Ok($"user id: {userId} and role: {role}");
    }
}