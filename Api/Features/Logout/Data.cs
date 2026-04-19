using Api.Data;
using Api.Extensions.Interfaces;
using Dapper;

namespace Api.Features.Logout;

internal class Data(DapperDbContext dbContext) : IRepository
{
    public async Task<bool> RemoveRefreshToken(Guid userId, string refreshToken, CancellationToken ct)
    {
        const string query = """
                             DELETE FROM 
                                        refresh_tokens
                             WHERE 
                                 user_id = @userId 
                               AND 
                                 token = @refreshToken
                             """;

        using var connection = dbContext.CreateConnection();

        return await connection.ExecuteAsync(
            new CommandDefinition(
                query,
                new
                {
                    userId,
                    refreshToken
                },
                cancellationToken: ct)
        ) > 0;
    }
}