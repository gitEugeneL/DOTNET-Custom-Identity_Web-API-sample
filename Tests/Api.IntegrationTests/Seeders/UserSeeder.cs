using Api.IntegrationTests.TestData;
using Api.Utils;
using Dapper;
using IdentityApi.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Api.IntegrationTests.Seeders;

public class UserSeeder(IPasswordService passwordService, IConfiguration configuration)
{
    public async Task<Guid?> SeedUser(TestUser user)
    {
        passwordService.CreatePasswordHash(user.Password, out var hash, out var salt);

        const string query = """
                             INSERT INTO users (email, pwd_hash, pwd_salt, role)
                             VALUES (@email, @pwd_hash, @pwd_salt, @role::role)
                             ON CONFLICT (email) DO NOTHING
                             RETURNING id
                             """;

        await using var connection = new NpgsqlConnection(configuration.GetConnectionString("PSQL"));
        return await connection.QueryFirstOrDefaultAsync<Guid?>(
            query,
            new
            {
                email = Normalizer.NormalizeImportantString(user.Email),
                pwd_hash = hash,
                pwd_salt = salt,
                role = user.Role.ToString()
            }
        );
    }
}