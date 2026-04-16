namespace Api.Features.Registration;

public sealed record Request(
    string Email,
    string Password,
    string ConfirmPassword);