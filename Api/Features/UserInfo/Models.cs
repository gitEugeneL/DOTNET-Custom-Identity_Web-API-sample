namespace Api.Features.UserInfo;

public sealed record UserInfoResponse(
    string Email,
    bool IsConfirmLocked,
    bool IsEmailConfirmed,
    DateTime? ConfirmLockExpires
);