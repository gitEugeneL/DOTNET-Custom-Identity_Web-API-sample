using Api.Constants;
using Api.Domain.Enums;
using Api.Extensions.Interfaces;
using Api.Features.Common;
using Api.Services.Interfaces;
using Api.Tools;
using Api.Utils;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.Refresh;

public class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiPaths.Refresh, HandleAsync)
            .AllowAnonymous();
    }

    private static async Task<Results<ValidationProblem, Ok<LoginOrRefreshResponse>, BadRequest<string>>> HandleAsync(
        RefreshOrLogoutRequest request,
        IValidator<RefreshOrLogoutRequest> validator,
        HttpContext httpContext,
        Data data,
        ITokenService tokenService,
        CancellationToken ct
    )
    {
        var validationErrors = await ValidationHelper.ValidateRequestAsync(request, validator);
        if (validationErrors is not null)
            return TypedResults.ValidationProblem(validationErrors);

        if (!Guid.TryParse(request.UserId, out var userId))
            return TypedResults.BadRequest(ApiMessages.InvalidIdFormatResultMessage(nameof(request.UserId)));

        if (!Enum.TryParse<Role>(request.ClientRole, true, out var role))
            return TypedResults.BadRequest(ApiMessages.InvalidIdFormatResultMessage(nameof(request.ClientRole)));

        // read refresh token (secure cookie)
        var userRefreshToken = CookieManager.ReadCookie(httpContext, request.ClientRole);
        var user = userRefreshToken is null
            ? null
            : await data.GetUserByRefreshToken(userId, role, userRefreshToken, ct);
        if (user is null)
            return TypedResults.BadRequest(ApiMessages.InvalidToken);

        var newRefreshToken = tokenService.GenerateRefreshToken(user);
        if (!await data.UpdateRefreshToken(userId, userRefreshToken!, newRefreshToken, ct))
            return TypedResults.BadRequest(ApiMessages.InvalidIdFormatResultMessage(nameof(request.UserId)));

        var newAccessToken = tokenService.GenerateAccessToken(user);

        // set new refresh token (secure cookie) 
        CookieManager.SetCookie(httpContext, newRefreshToken.Token, newRefreshToken.Expires, user.Role.ToString());

        return TypedResults.Ok(new LoginOrRefreshResponse(
            user.Id,
            newAccessToken.token,
            newAccessToken.expires,
            newRefreshToken.Expires
        ));
    }
}