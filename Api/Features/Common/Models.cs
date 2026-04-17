namespace Api.Features.Common;

public sealed record LoginOrRefreshResponse(
    Guid UserId,
    string AccessToken,
    DateTime AccessTokenExpires,
    DateTime RefreshTokenExpires,
    string AccessTokenType = "Bearer"
);

public sealed record RefreshOrLogoutRequest(string UserId, string ClientRole);