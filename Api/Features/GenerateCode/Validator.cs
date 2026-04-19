using FluentValidation;

namespace Api.Features.GenerateCode;

public sealed class Validator : AbstractValidator<GenerateCodeRequest>
{
    public Validator()
    {
        // --- Email ---
        RuleFor(request => request.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be valid email");
    }
}