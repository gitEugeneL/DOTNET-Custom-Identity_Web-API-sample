using Api.IntegrationTests.Seeders;
using Api.IntegrationTests.TestData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Api.IntegrationTests;

public static class FactoryExtensions
{
    extension(ApiWebApplicationFactory factory)
    {
        public async Task<Guid?> SeedUser(TestUser user)
        {
            using var scope = factory.Services.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<UserSeeder>();
            return await seeder.SeedUser(user);
        }

        public T GetRequiredConfig<T>(string key)
        {
            var config = factory.Services.GetRequiredService<IConfiguration>();

            var value = config[key];

            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException($"Configuration key '{key}' is missing.");

            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}