using Api.IntegrationTests.FakeServices;
using Api.IntegrationTests.Seeders;
using Api.Services.Interfaces;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace Api.IntegrationTests;

public class ApiWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder("postgres:18-alpine")
        .WithDatabase("api")
        .WithUsername("user")
        .WithPassword("password")
        .WithCleanUp(true)
        .Build();

    private Respawner _respawner = null!;

    public string ConnectionString = null!;

    public async Task InitializeAsync()
    {
        await _db.StartAsync();
        ConnectionString = _db.GetConnectionString();

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        var sql = await File.ReadAllTextAsync(
            Path.Combine(AppContext.BaseDirectory, "init_database.sql"));

        await connection.ExecuteAsync(sql);

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres
        });
    }

    public new async Task DisposeAsync()
    {
        await _db.StopAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, cfg) =>
            cfg.AddInMemoryCollection([
                // Add the connection string to the test database
                new KeyValuePair<string, string?>("ConnectionStrings:PSQL", ConnectionString),

                // Disable rate limiting (fake permitLimit)
                new KeyValuePair<string, string?>("RateLimiting:BasePermitLimit", "999999")
            ]));

        builder.ConfigureServices(services =>
        {
            // Add UserSeeder to the DI container for tests
            services.AddScoped<UserSeeder>();

            // Add fake confirmation service (generate confirm code)
            services.AddScoped<IConfirmationService, FakeConfirmationService>();
        });
    }
}