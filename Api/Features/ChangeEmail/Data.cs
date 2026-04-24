using Api.Data;
using Api.Domain.Entities;
using Api.Extensions.Interfaces;
using Dapper;

namespace Api.Features.ChangeEmail;

internal class Data(DapperDbContext dbContext) : IRepository
{
    public async Task<User?> GetUser(Guid userId, string email, CancellationToken ct)
    {
        const string query = """
                             SELECT 
                                 id,
                                 email,
                                 confirm_locked AS confirmLocked,
                                 confirm_lock_expires AS confirmLockExpires,
                                 confirm_failed_count AS confirmFailedCount,
                                 email_confirmed AS emailConfirmed
                             FROM 
                                 users 
                             WHERE 
                                 id = @userId 
                               AND 
                                 email = @email 
                               AND 
                                 email_confirmed = true 
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
                    userId,
                    email
                },
                cancellationToken: ct
            )
        );
    }

    public async Task<bool> IsEmailUnique(string email, CancellationToken ct)
    {
        const string query = """
                             SELECT EXISTS (
                                 SELECT 
                                    1
                                 FROM 
                                     users
                                 WHERE 
                                     email = @email);
                             """;

        using var connection = dbContext.CreateConnection();

        return !await connection.QuerySingleAsync<bool>(
            new CommandDefinition(
                query,
                new { email },
                cancellationToken: ct
            )
        );
    }


    public async Task<bool> ChangeUserEmail(User user, CancellationToken ct)
    {
        const string query = """
                             UPDATE 
                                 users 
                             SET
                                email = @email, 
                                email_confirmed = @emailConfirmed,
                                confirm_locked = @confirmLocked,
                                confirm_lock_expires = @confirmLockExpires,
                                confirm_failed_count = @confirmFailedCount,
                                generate_code_count = @generateCodeCount
                             WHERE 
                                 id = @Id 
                             """;

        using var connection = dbContext.CreateConnection();

        return await connection.ExecuteAsync(
            new CommandDefinition(
                query,
                new
                {
                    user.Email,
                    user.EmailConfirmed,
                    user.ConfirmLocked,
                    user.ConfirmLockExpires,
                    user.ConfirmFailedCount,
                    user.GenerateCodeCount,
                    user.Id
                },
                cancellationToken: ct
            )
        ) > 0;
    }

    public async Task RemoveUserRefreshTokens(Guid userId, CancellationToken ct)
    {
        const string query = """
                             DELETE FROM 
                                refresh_tokens 
                             WHERE 
                                user_id = @userId
                             """;

        using var connection = dbContext.CreateConnection();

        await connection.ExecuteAsync(
            new CommandDefinition(
                query,
                new { userId },
                cancellationToken: ct
            )
        );
    }
}