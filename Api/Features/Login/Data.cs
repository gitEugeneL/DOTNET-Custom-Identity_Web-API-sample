using Api.Data;
using Api.Domain.Entities;
using Api.Extensions.Interfaces;
using Dapper;

namespace Api.Features.Login;

internal class Data(DapperDbContext dbContext, IConfiguration configuration) : IRepository
{
    public async Task<User?> GetUser(string email, CancellationToken ct)
    {
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
                               role::text AS role
                             FROM 
                                 users
                             WHERE 
                                 email = @email 
                               AND 
                               (
                                   login_lock_expires IS NULL 
                                       OR 
                                   login_lock_expires < now()
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

    public async Task UpdateLoginLockout(User user, CancellationToken ct)
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

        await connection.ExecuteAsync(
            new CommandDefinition(query, new
            {
                loginLocked = user.LoginLocked,
                loginLockExpires = user.LoginLockExpires,
                loginFailedCount = user.LoginFailedCount,
                userId = user.Id
            }, cancellationToken: ct)
        );
    }

    public async Task AddRefreshToken(User user, RefreshToken refreshToken, CancellationToken ct)
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

        try
        {
            await connection.ExecuteAsync(new CommandDefinition(
                deleteOldestQuery,
                new
                {
                    userId = user.Id,
                    maxRefreshTokenCount
                },
                transaction,
                cancellationToken: ct));

            await connection.ExecuteAsync(new CommandDefinition(
                insertQuery,
                new
                {
                    token = refreshToken.Token,
                    expires = refreshToken.Expires,
                    userId = user.Id
                },
                transaction,
                cancellationToken: ct));

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}