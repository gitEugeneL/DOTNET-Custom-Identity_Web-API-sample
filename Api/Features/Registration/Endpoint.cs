using Api.Constants;
using Api.Domain.Entities;
using Api.Domain.Enums;
using Api.Extensions.Interfaces;
using Api.Utils;
using FluentValidation;
using IdentityApi.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.Registration;

public class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiPaths.Registration, HandleAsync)
            .AllowAnonymous();
    }

    private static async Task<Results<ValidationProblem, Conflict<string>, Created<string>>> HandleAsync(
        RegistrationRequest request,
        IValidator<RegistrationRequest> validator,
        IPasswordService passwordService,
        Data data,
        CancellationToken ct)
    {
        var validationErrors = await ValidationHelper.ValidateRequestAsync(request, validator);
        if (validationErrors is not null)
            return TypedResults.ValidationProblem(validationErrors);

        passwordService.CreatePasswordHash(request.Password, out var passwordHash, out var passwordSalt);

        var user = new User
        {
            Email = Normalizer.NormalizeImportantString(request.Email),
            PwdHash = passwordHash,
            PwdSalt = passwordSalt,
            Role = Role.Customer
        };

        var result = await data.Create(user);
        return result is null
            ? TypedResults.Conflict(ApiMessages.ConflictResultMessage(nameof(User), request.Email))
            : TypedResults.Created("", result.ToString());
    }
}