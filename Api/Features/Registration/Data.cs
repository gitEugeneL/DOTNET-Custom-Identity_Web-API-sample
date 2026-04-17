using Api.Data;
using Api.Domain.Entities;
using Api.Extensions.Interfaces;
using Dapper;

namespace Api.Features.Registration;

internal class Data(DapperDbContext dbContext) : IRepository
{
    public async Task<Guid?> Create(User user, CancellationToken ct)
    {
        const string query = """
                             INSERT INTO users (email, pwd_hash, pwd_salt, role)
                             VALUES (@email, @pwd_hash, @pwd_salt, @role::role)
                             ON CONFLICT (email) DO NOTHING
                             RETURNING id
                             """;

        using var connection = dbContext.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Guid?>(new CommandDefinition(query, new
            {
                email = user.Email,
                pwd_hash = user.PwdHash,
                pwd_salt = user.PwdSalt,
                role = user.Role.ToString()
            },
            cancellationToken: ct)
        );
    }
}