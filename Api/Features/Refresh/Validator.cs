using FluentValidation;

namespace Api.Features.Refresh;

public sealed class Validator : AbstractValidator<RefreshRequest>
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