using Api.Constants;
using Api.Extensions.Interfaces;
using Api.Services.Interfaces;
using Api.Utils;
using FluentValidation;
using IdentityApi.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.ChangePassword;

public class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiPaths.ChangePassword, HandleAsync)
            .AllowAnonymous();
    }

    private static async Task<Results<ValidationProblem, Ok<ChangePasswordResponse>, BadRequest<string>>> HandleAsync(
        ChangePasswordRequest request,
        IValidator<ChangePasswordRequest> validator,
        ILockoutService lockoutService,
        IPasswordService passwordService,
        Data data,
        Common.Data commonData,
        CancellationToken ct
    )
    {
        var validationErrors = await ValidationHelper.ValidateRequestAsync(request, validator);
        if (validationErrors is not null)
            return TypedResults.ValidationProblem(validationErrors);

        var user = await data.GetUser(Normalizer.NormalizeImportantString(request.Email), ct);
        if (user is null)
            return TypedResults.BadRequest(ApiMessages.InvalidConfirm);

        var isCodeValid = await commonData.IsCodeValidThenRemove(user.Id, request.Code, ct);
        lockoutService.ProcessForConfirm(user, isCodeValid);
        if (!isCodeValid)
        {
            await commonData.UpdateConfirmLockout(user, ct);
            return TypedResults.BadRequest(ApiMessages.InvalidConfirm);
        }

        passwordService.CreatePasswordHash(request.Password, out var passwordHash, out var passwordSalt);
        user.PwdHash = passwordHash;
        user.PwdSalt = passwordSalt;

        return await data.ChangePassword(user, ct)
            ? TypedResults.Ok(new ChangePasswordResponse(user.Email, true))
            : TypedResults.BadRequest(ApiMessages.InvalidConfirm);
    }
}