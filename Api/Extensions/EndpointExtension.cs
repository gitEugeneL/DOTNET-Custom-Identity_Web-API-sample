using System.Reflection;
using Api.Extensions.Interfaces;

namespace Api.Extensions;

public static class EndpointExtension
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services)
    {
        var endpointTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } &&
                        typeof(IEndpoint).IsAssignableFrom(t));

        foreach (var type in endpointTypes)
            services.AddScoped(type);

        return services;
    }
    
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        using var scope = app.ServiceProvider.CreateScope();
        var endpointTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } &&
                        typeof(IEndpoint).IsAssignableFrom(t));

        foreach (var type in endpointTypes)
        {
            var endpoint = (IEndpoint)scope.ServiceProvider.GetRequiredService(type);
            endpoint.MapEndpoint(app);
        }

        return app;
    }
}