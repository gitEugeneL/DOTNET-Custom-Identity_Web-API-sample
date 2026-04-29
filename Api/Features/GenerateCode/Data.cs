using Api.Data;
using Api.Domain.Entities;
using Api.Extensions.Interfaces;
using Dapper;

namespace Api.Features.GenerateCode;

internal class Data(DapperDbContext dbContext) : IRepository
{
    public async Task<User?> GetUser(string email, CancellationToken ct)
    {
        const string query = """
                             SELECT 
                               id,
                               email,
                               confirm_locked AS confirmLocked,
                               confirm_lock_expires AS confirmLockExpires,
                               confirm_failed_count AS confirmFailedCount,
                               generate_code_count AS generateCodeCount
                             FROM users
                             WHERE 
                                 email = @email  
                               AND 
                               (
                                   confirm_lock_expires IS NULL 
                                       OR 
                                   confirm_lock_expires < now()
                               )
                             """;

        using var connection = dbContext.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<User>(
            new CommandDefinition(
                query,
                new
                {
                    email
                },
                cancellationToken: ct)
        );
    }

    public async Task UpdateGenerateCodeLockout(User user, CancellationToken ct)
    {
        const string query = """
                             UPDATE users
                             SET
                                confirm_locked = @confirmLocked,
                                confirm_lock_expires = @confirmLockExpires,
                                generate_code_count = @generateCodeCount
                             WHERE 
                                 id = @userId
                             """;

        using var connection = dbContext.CreateConnection();

        await connection.ExecuteAsync(
            new CommandDefinition(query, new
                {
                    confirmLocked = user.ConfirmLocked,
                    confirmLockExpires = user.ConfirmLockExpires,
                    generateCodeCount = user.GenerateCodeCount,
                    userId = user.Id
                },
                cancellationToken: ct)
        );
    }

    public async Task<bool> UpdateUserConfirmationCode(ConfirmationCode confirmationCode, CancellationToken ct)
    {
        const string query = """
                             INSERT INTO confirmation_codes (code, expires, user_id)
                             VALUES (@code, @expires, @userId)
                             ON CONFLICT (user_id)
                             DO UPDATE SET
                                 code = EXCLUDED.code,
                                 expires = EXCLUDED.expires;
                             """;

        using var connection = dbContext.CreateConnection();

        return await connection.ExecuteAsync(
            new CommandDefinition(query, new
                {
                    code = confirmationCode.Code,
                    expires = confirmationCode.Expires,
                    userId = confirmationCode.UserId
                },
                cancellationToken: ct)
        ) > 0;
    }
}