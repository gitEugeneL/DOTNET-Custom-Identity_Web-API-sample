using Api.Domain.Entities;

namespace Api.Services.Interfaces;

public interface ITokenService
{
    (string token, DateTime expires) GenerateAccessToken(User user);

    RefreshToken GenerateRefreshToken(User user);
}