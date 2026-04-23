using FluentValidation;

namespace Api.Features.ConfirmEmail;

public class Validator : AbstractValidator<ConfirmEmailRequest>
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
    }
}