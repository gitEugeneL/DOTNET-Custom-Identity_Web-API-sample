namespace Api.Features.ConfirmEmail;

public sealed record ConfirmEmailRequest(string Code, string Email);

public sealed record ConfirmEmailResponse(string Email, bool IsConfirmed);