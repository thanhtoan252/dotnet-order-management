using FluentValidation;
using Inventory.Application;
using Inventory.Infrastructure;
using Inventory.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Shared.Web.Authentication;
using Shared.Web.Cors;
using Shared.Web.Extensions;
using Shared.Web.Middleware;

namespace Inventory.Api.Extensions;

internal static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddApplication()
            .AddInfrastructure(configuration);

        services.AddValidatorsFromAssemblyContaining<Program>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddUserPrinciple();
        services.AddProblemDetails();
        services.AddHealthChecks().AddDbContextCheck<InventoryDbContext>("database");
        services.AddInventoryApiVersioning();
        services.AddOpenApi();

        return services;
    }

    public static IServiceCollection AddJwtAuth(this IServiceCollection services, KeycloakJwtOptions keycloak)
    {
        var validIssuer = keycloak.ValidIssuer ?? keycloak.Authority;

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = keycloak.Authority;
                options.RequireHttpsMetadata = false;
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = validIssuer,
                    ValidateAudience = false,
                    RoleClaimType = "roles"
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("inventory:read", p => p.RequireAuthenticatedUser());
            options.AddPolicy("inventory:adjust", p => p.RequireRole("admin"));
        });

        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, AppCorsOptions corsOptions,
        IWebHostEnvironment environment)
    {
        services.AddCors(options =>
        {
            if (environment.IsDevelopment() || corsOptions.AllowedOrigins.Length == 0)
            {
                options.AddPolicy("CorsPolicy", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            }
            else
            {
                options.AddPolicy("CorsPolicy",
                    p => p.WithOrigins(corsOptions.AllowedOrigins).AllowAnyMethod().AllowAnyHeader());
            }
        });

        return services;
    }
}
