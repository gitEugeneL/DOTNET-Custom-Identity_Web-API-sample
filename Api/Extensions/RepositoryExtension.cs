using Api.Extensions.Interfaces;

namespace Api.Extensions;

public static class RepositoryExtension
{
    public static IServiceCollection AddRepoServices(this IServiceCollection services)
    {
        var repositories = typeof(Program).Assembly
            .GetTypes()
            .Where(t => typeof(IRepository).IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false });

        foreach (var repository in repositories)
            services.AddScoped(repository);

        return services;
    }
}