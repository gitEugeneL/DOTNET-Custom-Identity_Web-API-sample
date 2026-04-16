using Api.Data;
using Api.Domain.Entities;
using Api.Extensions.Interfaces;
using Dapper;

namespace Api.Features.Login;

internal class Data(DapperDbContext dbContext, IConfiguration configuration) : IRepository
{
    public async Task<User?> GetUser(string email)
    {
        // TODO: Add additional checks:
        // AND
        //     email_confirmed = true

        const string query = """
                             SELECT 
                               id,
                               email,
                               pwd_hash AS pwdHash,
                               pwd_salt AS pwdSalt,
                               email_confirmed AS emailConfirmed,
                               login_failed_count AS loginFailedCount,
                               login_locked AS loginLocked,
                               login_lock_expires AS loginLockExpires,
                               confirm_locked AS confirmLocked,
                               confirm_lock_expires AS confirmLockExpires,
                               confirm_failed_count AS confirmFailedCount,
                               generate_code_count AS generateCodeCount,
                               role
                             FROM users u 
                             WHERE 
                                 email = @email 
                             """;

        using var connection = dbContext.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<User>(query, new { email });
    }

    public async Task UpdateLoginLockout(User user)
    {
        const string query = """
                             UPDATE users
                             SET
                                login_locked = @loginLocked,
                                login_lock_expires = @loginLockExpires,
                                login_failed_count = @loginFailedCount
                             WHERE 
                                 id = @userId
                             """;

        using var connection = dbContext.CreateConnection();
        await connection.ExecuteAsync(query, new
        {
            loginLocked = user.LoginLocked,
            loginLockExpires = user.LoginLockExpires,
            loginFailedCount = user.LoginFailedCount,
            userId = user.Id
        });
    }

    public async Task UpdateRefreshToken(User user, RefreshToken refreshToken)
    {
        var maxRefreshTokenCount = int.Parse(configuration["Authentication:RefreshToken.MaxCount"]!);

        const string deleteOldestQuery = """
                                         DELETE FROM refresh_tokens
                                         WHERE id = (
                                             SELECT id
                                             FROM refresh_tokens
                                             WHERE user_id = @userId
                                             ORDER BY expires
                                             LIMIT 1
                                         )
                                         AND (
                                             SELECT COUNT(*)
                                             FROM refresh_tokens
                                             WHERE user_id = @userId
                                         ) >= @maxRefreshTokenCount;
                                         """;

        const string insertQuery = """
                                   INSERT INTO refresh_tokens (token, expires, user_id)
                                   VALUES (@token, @expires, @userId);
                                   """;

        using var connection = dbContext.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(deleteOldestQuery, new
            {
                userId = user.Id,
                maxRefreshTokenCount
            },
            transaction);

        await connection.ExecuteAsync(insertQuery, new
            {
                token = refreshToken.Token,
                expires = refreshToken.Expires,
                userId = user.Id
            },
            transaction);

        transaction.Commit();
    }
}