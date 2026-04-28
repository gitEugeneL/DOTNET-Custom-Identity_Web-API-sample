using Api.Data;
using Api.Domain.Entities;
using Api.Extensions.Interfaces;
using Dapper;

namespace Api.Features.UserInfo;

internal class Data(DapperDbContext dbContext) : IRepository
{
    public async Task<User?> GetUser(Guid userId, CancellationToken ct)
    {
        const string query = """
                             SELECT
                                id,
                                email,
                                email_confirmed AS emailConfirmed,
                                login_locked AS loginLocked,
                                confirm_locked AS confirmLocked,
                                login_lock_expires AS loginLockExpires,
                                confirm_lock_expires AS confirmLockExpires,
                                role::text AS role
                             FROM
                                 users
                             WHERE 
                                 id = @userId
                             """;

        using var connection = dbContext.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<User>(
            new CommandDefinition(
                query,
                new { userId },
                cancellationToken: ct)
        );
    }
}