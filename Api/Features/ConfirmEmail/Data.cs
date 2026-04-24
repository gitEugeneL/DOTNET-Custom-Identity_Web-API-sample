using Api.Data;
using Api.Domain.Entities;
using Api.Extensions.Interfaces;
using Dapper;

namespace Api.Features.ConfirmEmail;

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
                               generate_code_count AS generateCodeCount,
                               email_confirmed AS emailConfirmed
                             FROM 
                                 users
                             WHERE 
                                 email = @email 
                               AND 
                                 email_confirmed = false
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
                new { email },
                cancellationToken: ct)
        );
    }

    public async Task ConfirmUser(User user, CancellationToken ct)
    {
        const string query = """
                             UPDATE
                                users
                             SET
                                confirm_locked = @confirmLocked,
                                confirm_lock_expires = @confirmLockExpires,
                                confirm_failed_count = @confirmFailedCount,
                                generate_code_count = @generateCodeCount,
                                email_confirmed = @emailConfirmed

                             WHERE 
                                 id = @userId
                             """;

        using var connection = dbContext.CreateConnection();

        await connection.ExecuteAsync(
            new CommandDefinition(
                query,
                new
                {
                    confirmLocked = user.ConfirmLocked,
                    confirmLockExpires = user.ConfirmLockExpires,
                    confirmFailedCount = user.ConfirmFailedCount,
                    generateCodeCount = user.GenerateCodeCount,
                    emailConfirmed = user.EmailConfirmed,
                    userId = user.Id
                },
                cancellationToken: ct)
        );
    }
}