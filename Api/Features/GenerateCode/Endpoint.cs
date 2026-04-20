using Api.Constants;
using Api.Domain.Entities;
using Api.Extensions.Interfaces;
using Api.Services.Interfaces;
using Api.Utils;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.GenerateCode;

public class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiPaths.GenerateCode, HandleAsync)
            .AllowAnonymous()
            .RequireRateLimiting(RateLimitPolicies.BasePolicy);
    }

    private static async Task<Results<ValidationProblem, Ok<GenerateCodeResponse>, BadRequest<string>>> HandleAsync(
        GenerateCodeRequest request,
        IValidator<GenerateCodeRequest> validator,
        ILockoutService lockoutService,
        IConfirmationService confirmationService,
        IMessageService messageService,
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

        lockoutService.ProcessForGenerateCode(user);
        await data.UpdateGenerateCodeLockout(user, ct);

        var (code, expires) = confirmationService.GenerateCode();
        var confirmationCode = new ConfirmationCode
        {
            Code = code,
            Expires = expires,
            UserId = user.Id
        };

        if (!await data.UpdateUserConfirmationCode(confirmationCode, ct))
            return TypedResults.BadRequest(ApiMessages.InvalidConfirm);

        await messageService.SendMessageAsync(user.Email, "Confirm your email", code, expires);
        return TypedResults.Ok(new GenerateCodeResponse(user.Email, expires));
    }
}