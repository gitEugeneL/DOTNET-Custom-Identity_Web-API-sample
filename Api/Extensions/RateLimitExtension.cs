using Api.Constants;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Extensions;

public static class RateLimitExtension
{
    public static IServiceCollection AddRateLimitingServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddFixedWindowLimiter(RateLimitPolicies.BasePolicy, limiterOptions =>
            {
                limiterOptions.PermitLimit = int.Parse(configuration["RateLimiting:BasePermitLimit"] ??
                                                       throw new ApplicationException(
                                                           "RateLimiting:BasePermitLimit not found in configuration"));
                limiterOptions.Window = TimeSpan.FromSeconds(1);
                limiterOptions.QueueLimit = 0;
                limiterOptions.AutoReplenishment = true;
            });
        });

        return services;
    }
}