using Api.Data;
using Api.Domain.Entities;
using Api.Domain.Enums;
using Api.Extensions.Interfaces;
using Dapper;

namespace Api.Features.Refresh;

internal class Data(DapperDbContext dbContext) : IRepository
{
    public async Task<User?> GetUserByRefreshToken(Guid userId, Role role, string refreshToken, CancellationToken ct)
    {
        const string query = """
                             SELECT
                                 u.id,
                                 u.email,
                                 u.role
                             FROM 
                                 users u
                             INNER JOIN 
                                     refresh_tokens rt 
                                 ON 
                                     rt.user_id = u.id
                             WHERE 
                                 u.id = @userId 
                               AND 
                                u.email_confirmed 
                               AND
                                 u.role = @role::role 
                               AND
                                 rt.token = @refreshToken
                             """;

        using var connection = dbContext.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<User>(
            new CommandDefinition(query, new
                {
                    userId,
                    role = role.ToString(),
                    refreshToken
                },
                cancellationToken: ct)
        );
    }

    public async Task<bool> UpdateRefreshToken(
        Guid userId,
        string oldRefreshToken,
        RefreshToken newRefreshToken,
        CancellationToken ct)
    {
        const string query = """
                             UPDATE 
                                 refresh_tokens
                             SET 
                                 token = @newToken,
                                 expires = @newExpires
                             WHERE 
                                 token = @oldRefreshToken 
                               AND
                                 user_id = @userId 
                               AND
                                 expires > now()
                             """;

        using var connection = dbContext.CreateConnection();

        return await connection.ExecuteAsync(
            new CommandDefinition(
                query,
                new
                {
                    newToken = newRefreshToken.Token,
                    newExpires = newRefreshToken.Expires,
                    oldRefreshToken,
                    userId
                },
                cancellationToken: ct)
        ) > 0;
    }
}