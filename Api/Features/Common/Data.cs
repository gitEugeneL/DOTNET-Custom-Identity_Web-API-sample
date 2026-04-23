using Api.Data;
using Api.Domain.Entities;
using Api.Extensions.Interfaces;
using Dapper;

namespace Api.Features.Common;

public class Data(DapperDbContext dbContext) : IRepository
{
    public async Task<bool> IsCodeValidThenRemove(Guid userId, string code, CancellationToken ct)
    {
        const string query = """
                             DELETE FROM 
                                        confirmation_codes
                             WHERE 
                                 user_id = @userId 
                               AND
                                 expires > now() 
                               AND
                                 code = @code
                             """;

        using var connection = dbContext.CreateConnection();

        return await connection.ExecuteAsync(
            new CommandDefinition(
                query,
                new
                {
                    userId,
                    code
                },
                cancellationToken: ct
            )
        ) > 0;
    }

    public async Task UpdateConfirmLockout(User user, CancellationToken ct, bool isConfirm = false)
    {
        const string query = """
                             UPDATE 
                                 users
                             SET
                                confirm_locked = @confirmLocked,
                                confirm_lock_expires = @confirmLockExpires,
                                confirm_failed_count = @confirmFailedCount
                             WHERE 
                                 id = @userId
                             """;

        using var connection = dbContext.CreateConnection();

        await connection.ExecuteAsync(
            new CommandDefinition(query, new
                {
                    confirmLocked = user.ConfirmLocked,
                    confirmLockExpires = user.ConfirmLockExpires,
                    confirmFailedCount = user.ConfirmFailedCount,
                    userId = user.Id
                },
                cancellationToken: ct)
        );
    }
}