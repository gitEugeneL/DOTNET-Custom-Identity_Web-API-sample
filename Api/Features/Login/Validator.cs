using FluentValidation;

namespace Api.Features.Login;

public sealed class Validator : AbstractValidator<Request>
{
    public Validator()
    {
        // --- Email ---
        RuleFor(request => request.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be valid email");

        // --- Password ---
        RuleFor(request => request.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .Length(8, 150)
            .WithMessage("Password must be between 8 and 150 characters");
    }
}