namespace Api.Features.Login;

public sealed record Request(
    string Email,
    string Password);

public sealed record Response(
    Guid UserId,
    string AccessToken,
    DateTime AccessTokenExpires,
    DateTime RefreshTokenExpires,
    string AccessTokenType = "Bearer"
);