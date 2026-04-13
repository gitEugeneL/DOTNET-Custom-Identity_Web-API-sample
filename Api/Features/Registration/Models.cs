namespace Api.Features.Registration;

public sealed record RegistrationRequest(
    string Email,
    string Password,
    string ConfirmPassword);