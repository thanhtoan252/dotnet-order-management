using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Order.Application;
using Order.Infrastructure;
using Order.Infrastructure.Data;
using Shared.Web.Authentication;
using Shared.Web.Cors;
using Shared.Web.Extensions;
using Shared.Web.Middleware;

namespace Order.Api.Extensions;

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
        services.AddHealthChecks().AddDbContextCheck<OrderDbContext>("database");
        services.AddOrderApiVersioning();
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
            options.AddPolicy("order:confirm", p => p.RequireRole("admin"));
            options.AddPolicy("order:ship", p => p.RequireRole("admin"));
            options.AddPolicy("order:deliver", p => p.RequireRole("admin"));
            options.AddPolicy("order:delete", p => p.RequireRole("admin"));
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
