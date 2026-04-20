using FluentValidation;

namespace Api.Features.ConfirmEmail;

public class Validator : AbstractValidator<ConfirmEmailRequest>
{
    public Validator(IConfiguration configuration)
    {
        var codeLength = int.Parse(configuration["Authentication:Code.Length"]!);

        // --- Email ---
        RuleFor(request => request.Email)
            .NotEmpty()
            .WithMessage("Email is required");

        // --- Code ---
        RuleFor(c => c.Code)
            .NotEmpty()
            .WithMessage("Code is required")
            .Length(codeLength)
            .WithMessage("Code is invalid");
    }
}