using FluentValidation;

namespace Api.Features.ResetPassword;

public class Validator : AbstractValidator<ChangePasswordRequest>
{
    public Validator(IConfiguration configuration)
    {
        var codeLength = int.Parse(configuration["Authentication:Code.Length"] ??
                                   throw new ApplicationException("Code.Length not found in configuration"));

        // --- Email ---
        RuleFor(request => request.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be valid email");

        // --- Code ---
        RuleFor(c => c.Code)
            .NotEmpty()
            .WithMessage("Code is required")
            .Length(codeLength)
            .WithMessage("Code is invalid");

        // --- Password ---
        RuleFor(request => request.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .Length(8, 150)
            .WithMessage("Password must be between 8 and 150 characters")
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
            .WithMessage("Confirm password is required")
            .Equal(command => command.Password)
            .WithMessage("Passwords do not match");
    }
}