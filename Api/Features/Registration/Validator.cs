using FluentValidation;

namespace Api.Features.Registration;

public sealed class Validator : AbstractValidator<RegistrationRequest>
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
            .Length(8, 20)
            .WithMessage("Password must be between 8 and 20 characters")
            .Must(p => p.Any(char.IsLetter))
            .WithMessage("Password must contain letters")
            .Must(p => p.Any(char.IsUpper))
            .WithMessage("Password must contain upper case")
            .Must(p => p.Any(char.IsDigit))
            .WithMessage("Password must contain digits")
            .Must(p => p.Any(c => !char.IsLetterOrDigit(c)))
            .WithMessage("Password must contain special characters");

        // --- Confirm Password ---
        RuleFor(request => request.ConfirmPassword)
            .NotEmpty()
            .Equal(command => command.Password)
            .WithMessage("Passwords do not match");
    }
}