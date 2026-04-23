using Api.Data;
using Api.Domain.Entities;
using Api.Extensions.Interfaces;
using Dapper;

namespace Api.Features.ResetPassword;

internal class Data(DapperDbContext dbContext) : IRepository
{
    public async Task<User?> GetUser(string email, CancellationToken ct)
    {
        const string query = """
                             SELECT 
                                 id,
                                 email,
                                 pwd_hash AS pwdHash,
                                 pwd_salt AS pwdSalt,
                                 confirm_locked AS confirmLocked,
                                 confirm_lock_expires AS confirmLockExpires,
                                 confirm_failed_count AS confirmFailedCount,
                                 email_confirmed AS emailConfirmed
                             FROM 
                                 users
                             WHERE 
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
                new { email },
                cancellationToken: ct
            )
        );
    }

    public async Task<bool> ChangePassword(User user, CancellationToken ct)
    {
        const string query = """
                             UPDATE 
                                users 
                             SET
                                 pwd_hash = @pwdHash,
                                 pwd_salt = @pwdSalt ,
                                 confirm_locked = @confirmLocked,
                                 confirm_lock_expires = @confirmLockExpires,
                                 confirm_failed_count = @confirmFailedCount,
                                 generate_code_count = @generateCodeCount
                             WHERE
                                id = @userId
                             """;

        using var connection = dbContext.CreateConnection();

        return await connection.ExecuteAsync(
            new CommandDefinition(query, new
                {
                    pwdHash = user.PwdHash,
                    pwdSalt = user.PwdSalt,
                    confirmLocked = user.ConfirmLocked,
                    confirmLockExpires = user.ConfirmLockExpires,
                    confirmFailedCount = user.ConfirmFailedCount,
                    generateCodeCount = user.GenerateCodeCount,
                    userId = user.Id
                }
            )) > 0;
    }
}