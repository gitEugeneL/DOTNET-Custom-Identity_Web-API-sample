using Api.Constants;
using Api.Extensions.Interfaces;
using Api.Services.Interfaces;
using Api.Utils;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.ConfirmEmail;

public class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiPaths.ConfirmEmail, HandleAsync)
            .AllowAnonymous();
    }

    private static async Task<Results<ValidationProblem, Ok<ConfirmEmailResponse>, BadRequest<string>>> HandleAsync(
        ConfirmEmailRequest request,
        IValidator<ConfirmEmailRequest> validator,
        ILockoutService lockoutService,
        IConfirmationService confirmationService,
        Data data,
        CancellationToken ct
    )
    {
        var validationErrors = await ValidationHelper.ValidateRequestAsync(request, validator);
        if (validationErrors is not null)
            return TypedResults.ValidationProblem(validationErrors);

        var user = await data.GetUser(Normalizer.NormalizeImportantString(request.Email), ct);
        if (user is null)
            return TypedResults.BadRequest(ApiMessages.InvalidConfirm);

        var isCodeValid = await data.IsCodeValidThenRemove(user.Id, request.Code, ct);
        lockoutService.ProcessForConfirm(user, isCodeValid);
        if (!isCodeValid)
        {
            await data.UpdateConfirmLockout(user, ct);
            return TypedResults.BadRequest(ApiMessages.InvalidConfirm);
        }

        await data.ConfirmUser(user, ct);

        return TypedResults.Ok(new ConfirmEmailResponse(user.Email, true));
    }
}