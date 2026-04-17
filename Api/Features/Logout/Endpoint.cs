using Api.Constants;
using Api.Extensions.Interfaces;
using Api.Features.Common;
using Api.Tools;
using Api.Utils;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.Logout;

public class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiPaths.Logout, HandleAsync);
    }

    private static async Task<Results<ValidationProblem, NoContent, BadRequest<string>>> HandleAsync(
        RefreshOrLogoutRequest request,
        IValidator<RefreshOrLogoutRequest> validator,
        HttpContext httpContext,
        Data data,
        CancellationToken ct
    )
    {
        var validationErrors = await ValidationHelper.ValidateRequestAsync(request, validator);
        if (validationErrors is not null)
            return TypedResults.ValidationProblem(validationErrors);

        if (!Guid.TryParse(request.UserId, out var userId))
            return TypedResults.BadRequest(ApiMessages.InvalidIdFormatResultMessage(nameof(request.UserId)));

        var userRefreshToken = CookieManager.ReadCookie(httpContext, request.ClientRole);
        if (userRefreshToken is null || !await data.RemoveRefreshToken(userId, userRefreshToken, ct))
            return TypedResults.BadRequest(ApiMessages.InvalidToken);

        CookieManager.RemoveCookie(httpContext, request.ClientRole);
        return TypedResults.NoContent();
    }
}