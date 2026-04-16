using Api.Constants;
using Api.Extensions.Interfaces;
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

    private static async Task<Results<ValidationProblem, Ok<Response>, BadRequest<string>>> HandleAsync(
        Request request,
        IValidator<Request> validator,
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

        var user = await data.GetUser(Normalizer.NormalizeImportantString(request.Email));

        if (user is null || lockoutService.IsLoginLocked(user))
            return TypedResults.BadRequest(ApiMessages.InvalidAuthMessage());

        if (lockoutService.IsLoginAttemptLimitExceeded(user))
        {
            await data.UpdateLoginLockout(user);
            return TypedResults.BadRequest(ApiMessages.InvalidAuthMessage());
        }

        if (!passwordService.VerifyPasswordHash(request.Password, user.PwdHash, user.PwdSalt))
        {
            user.LoginFailedCount++;
            await data.UpdateLoginLockout(user);
            return TypedResults.BadRequest(ApiMessages.InvalidAuthMessage());
        }

        lockoutService.ResetLoginLockout(user);
        await data.UpdateLoginLockout(user);

        var newAccessToken = tokenService.GenerateAccessToken(user);
        var newRefreshToken = tokenService.GenerateRefreshToken(user);

        await data.UpdateRefreshToken(user, newRefreshToken);

        CookieSetter.SetCookie(httpContext, newRefreshToken.Token, newRefreshToken.Expires, user.Role.ToString());

        return TypedResults.Ok(new Response(
            user.Id,
            newAccessToken.token,
            newAccessToken.expires,
            newRefreshToken.Expires
        ));
    }
}