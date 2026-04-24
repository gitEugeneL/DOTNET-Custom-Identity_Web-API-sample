using System.Security.Claims;
using System.Text;
using Api.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace Api.Extensions;

public static class AuthExtension
{
    public static void ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(3),

                    ValidIssuer = configuration["Authentication:Issuer"] ??
                                  throw new ApplicationException("Issuer not found in configuration"),

                    ValidAudience = configuration["Authentication:Audience"] ??
                                    throw new ApplicationException("Audience not found in configuration"),

                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                        .GetBytes(configuration["Authentication:AccessToken.SecurityKey"] ??
                                  throw new ApplicationException("SecurityKey not found in configuration")))
                };
            });
    }

    public static void ConfigureAuthPolicy(this IServiceCollection services)
    {
        var commonPolicy = new AuthorizationPolicyBuilder()
            .RequireClaim(ClaimTypes.Email)
            .RequireClaim(ClaimTypes.NameIdentifier)
            .RequireClaim(ClaimTypes.Role)
            .RequireClaim("isEmailConfirmed")
            .Build();

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthPolicies.BasePolicy, commonPolicy)
            .AddPolicy(AuthPolicies.AdminPolicy, policy =>
            {
                policy
                    .RequireRole(AuthPolicies.Admin)
                    .AddRequirements(commonPolicy.Requirements.ToArray());
            })
            .AddPolicy(AuthPolicies.CustomerPolicy, policy =>
            {
                policy
                    .RequireRole(AuthPolicies.Customer)
                    .AddRequirements(commonPolicy.Requirements.ToArray());
            });
    }
}