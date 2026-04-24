namespace Api.Features.ChangeEmail;

public sealed record ChangeEmailRequest(
    string NewEmail,
    string Code
);

public sealed record ChangeEmailResponse(
    string NewEmail,
    bool IsEmailChanged
);