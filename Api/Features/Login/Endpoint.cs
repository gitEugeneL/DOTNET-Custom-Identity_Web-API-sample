using Api.Constants;
using Api.Extensions.Interfaces;
using Api.Features.Common;
using Api.Services.Interfaces;
using Api.Tools;
using Api.Utils;
using FluentValidation;
using IdentityApi.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.Login;

public class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiPaths.Login, HandleAsync)
            .AllowAnonymous();
    }

    private static async Task<Results<ValidationProblem, Ok<LoginOrRefreshResponse>, BadRequest<string>>> HandleAsync(
        LoginRequest request,
        IValidator<LoginRequest> validator,
        IPasswordService passwordService,
        ILockoutService lockoutService,
        ITokenService tokenService,
        HttpContext httpContext,
        Data data,
        CancellationToken ct)
    {
        var validationErrors = await ValidationHelper.ValidateRequestAsync(request, validator);
        if (validationErrors is not null)
            return TypedResults.ValidationProblem(validationErrors);

        var user = await data.GetUser(Normalizer.NormalizeImportantString(request.Email), ct);

        if (user is null || lockoutService.IsLoginLocked(user))
            return TypedResults.BadRequest(ApiMessages.InvalidAuth);

        if (lockoutService.IsLoginAttemptLimitExceeded(user))
        {
            await data.UpdateLoginLockout(user, ct);
            return TypedResults.BadRequest(ApiMessages.InvalidAuth);
        }

        if (!passwordService.VerifyPasswordHash(request.Password, user.PwdHash, user.PwdSalt))
        {
            user.LoginFailedCount++;
            await data.UpdateLoginLockout(user, ct);
            return TypedResults.BadRequest(ApiMessages.InvalidAuth);
        }

        lockoutService.ResetLoginLockout(user);
        await data.UpdateLoginLockout(user, ct);

        var newAccessToken = tokenService.GenerateAccessToken(user);
        var newRefreshToken = tokenService.GenerateRefreshToken(user);

        await data.AddRefreshToken(user, newRefreshToken, ct);

        // set refresh token (secure cookie) 
        CookieManager.SetCookie(httpContext, newRefreshToken.Token, newRefreshToken.Expires, user.Role.ToString());

        return TypedResults.Ok(new LoginOrRefreshResponse(
            user.Id,
            newAccessToken.token,
            newAccessToken.expires,
            newRefreshToken.Expires
        ));
    }
}