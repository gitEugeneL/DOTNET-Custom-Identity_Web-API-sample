namespace Api.Features.Login;

public sealed record LoginRequest(
    string Email,
    string Password);