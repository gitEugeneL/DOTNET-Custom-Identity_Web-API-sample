using Api.Constants;
using Api.Domain.Entities;
using Api.Extensions.Interfaces;
using Api.Services.Interfaces;
using Api.Tools;
using Api.Utils;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.ChangeEmail;

public class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiPaths.ChangeEmail, HandleAsync)
            .RequireAuthorization(AuthPolicies.BasePolicy);
    }

    private static async
        Task<Results<ValidationProblem, UnauthorizedHttpResult, Conflict<string>, Ok<ChangeEmailResponse>,
            BadRequest<string>>>
        HandleAsync(
            ChangeEmailRequest request,
            IValidator<ChangeEmailRequest> validator,
            HttpContext httpContext,
            ILockoutService lockoutService,
            Common.Data commonData,
            Data data,
            CancellationToken ct
        )
    {
        var validationErrors = await ValidationHelper.ValidateRequestAsync(request, validator);
        if (validationErrors is not null)
            return TypedResults.ValidationProblem(validationErrors);

        var userId = JwtReader.ReadUserId(httpContext);
        var userEmail = JwtReader.ReadUserEmail(httpContext);
        var userRole = JwtReader.ReadUserRole(httpContext);

        if (userId is null || userEmail is null || userRole is null)
            return TypedResults.Unauthorized();

        var user = await data.GetUser(userId.Value, Normalizer.NormalizeImportantString(userEmail), ct);
        if (user is null)
            return TypedResults.BadRequest(ApiMessages.InvalidConfirm);


        var newEmail = Normalizer.NormalizeImportantString(request.NewEmail);
        if (!await data.IsEmailUnique(newEmail, ct))
            return TypedResults.Conflict(ApiMessages.ConflictResultMessage(nameof(User), request.NewEmail));

        var isCodeValid = await commonData.IsCodeValidThenRemove(user.Id, request.Code, ct);
        lockoutService.ProcessForConfirm(user, isCodeValid);
        if (!isCodeValid)
        {
            await commonData.UpdateConfirmLockout(user, ct);
            return TypedResults.BadRequest(ApiMessages.InvalidConfirm);
        }

        user.Email = newEmail;
        user.EmailConfirmed = false;

        await data.RemoveUserRefreshTokens(userId.Value, ct);
        CookieManager.RemoveCookie(httpContext, userRole);

        return await data.ChangeUserEmail(user, ct)
            ? TypedResults.Ok(new ChangeEmailResponse(user.Email, true))
            : TypedResults.BadRequest(ApiMessages.InvalidConfirm);
    }
}