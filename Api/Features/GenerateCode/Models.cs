namespace Api.Features.GenerateCode;

public sealed record GenerateCodeRequest(string Email);

public sealed record GenerateCodeResponse(string Email, DateTime CodeExpires);