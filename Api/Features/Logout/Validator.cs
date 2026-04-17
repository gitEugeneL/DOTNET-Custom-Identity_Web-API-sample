using Api.Features.Common;
using FluentValidation;

namespace Api.Features.Logout;

public sealed class Validator : AbstractValidator<RefreshOrLogoutRequest>
{
    public Validator()
    {
        RuleFor(request => request.UserId)
            .NotEmpty()
            .WithMessage("userId is required");

        RuleFor(request => request.ClientRole)
            .NotEmpty()
            .WithMessage("role is required");
    }
}