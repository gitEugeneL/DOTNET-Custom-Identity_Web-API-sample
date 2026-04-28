namespace Api.Features.ChangePassword;

public sealed record ChangePasswordRequest(
    string Email,
    string Code,
    string Password,
    string ConfirmPassword
);

public sealed record ChangePasswordResponse(
    string Email,
    bool IsPasswordChanged
);