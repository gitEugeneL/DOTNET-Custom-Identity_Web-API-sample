using Api.Constants;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Extensions;

public static class RateLimitExtension
{
    public static IServiceCollection AddRateLimitingServices(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddFixedWindowLimiter(RateLimitPolicies.BasePolicy, limiterOptions =>
            {
                limiterOptions.PermitLimit = 1;
                limiterOptions.Window = TimeSpan.FromSeconds(1);
                limiterOptions.QueueLimit = 0;
                limiterOptions.AutoReplenishment = true;
            });
        });

        return services;
    }
}