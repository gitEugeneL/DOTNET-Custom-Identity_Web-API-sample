using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Api.Domain.Entities;
using Api.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Api.Services;

public class TokenService(IConfiguration configuration) : ITokenService
{
    public (string token, DateTime expires) GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("isEmailConfirmed", user.EmailConfirmed.ToString())
        };

        var settings = configuration["Authentication:AccessToken.SecurityKey"]!;
        var expires =
            DateTime.UtcNow.AddMinutes(int.Parse(configuration["Authentication:AccessToken.Lifetime.Minutes"]!));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            SigningCredentials = credentials,
            Issuer = configuration["Authentication:Issuer"],
            Audience = configuration["Authentication:Audience"]
        };
        var handler = new JwtSecurityTokenHandler();
        var token = handler.WriteToken(handler.CreateToken(descriptor));

        return (token, expires);
    }

    public bool IsRefreshTokenActive(RefreshToken refreshToken)
    {
        return refreshToken.Expires >= DateTime.UtcNow;
    }

    public RefreshToken GenerateRefreshToken(User user)
    {
        return new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(265)),
            Expires = DateTime.UtcNow.AddDays(
                int.Parse(configuration["Authentication:RefreshToken.Lifetime.Days"]!)),
            UserId = user.Id
        };
    }
}