using System.Security.Cryptography;
using System.Text;
using IdentityApi.Services.Interfaces;

namespace Api.Services;

public sealed class PasswordService : IPasswordService
{
    public void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
    {
        using var hmac = new HMACSHA512();
        hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        salt = hmac.Key;
    }

    public bool VerifyPasswordHash(string password, byte[] hash, byte[] salt)
    {
        using var hmac = new HMACSHA512(salt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return computedHash.SequenceEqual(hash);
    }
}