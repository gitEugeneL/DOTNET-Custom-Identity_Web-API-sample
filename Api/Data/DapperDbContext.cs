using System.Data;
using Npgsql;

namespace Api.Data;

public class DapperDbContext(IConfiguration configuration)
{
    private readonly string _psqlConnectionString = configuration
        .GetConnectionString("PSQL") ?? throw new ApplicationException("PSQL connection string is null");


    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(_psqlConnectionString);
    }
}