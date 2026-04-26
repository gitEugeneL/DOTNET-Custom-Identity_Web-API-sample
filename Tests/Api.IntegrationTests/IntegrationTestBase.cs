namespace Api.IntegrationTests;

public abstract class IntegrationTestBase(ApiWebApplicationFactory factory) : IAsyncLifetime
{
    protected readonly HttpClient Client = factory.CreateClient();
    protected readonly ApiWebApplicationFactory Factory = factory;

    public async Task InitializeAsync()
    {
        await Factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}