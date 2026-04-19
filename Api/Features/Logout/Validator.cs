using Api.Features.Common;
using FluentValidation;

namespace Api.Features.Logout;

public sealed class Validator : AbstractValidator<RefreshOrLogoutRequest>
{
    public Validator()
    {
        // --- UserId ---
        RuleFor(request => request.UserId)
            .NotEmpty()
            .WithMessage("userId is required");

        // --- ClientRole ---
        RuleFor(request => request.ClientRole)
            .NotEmpty()
            .WithMessage("role is required");
    }
}