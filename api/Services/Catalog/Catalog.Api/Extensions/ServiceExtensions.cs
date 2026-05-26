using Catalog.Application;
using Catalog.Infrastructure;
using Catalog.Infrastructure.Data;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;
using Shared.Web.Authentication;
using Shared.Web.Cors;
using Shared.Web.Extensions;
using Shared.Web.Middleware;

namespace Catalog.Api.Extensions;

internal static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddApplication()
            .AddInfrastructure(configuration);

        services.Configure<FormOptions>(o => o.MultipartBodyLengthLimit = 5 * 1024 * 1024);
        services.AddValidatorsFromAssemblyContaining<Program>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddUserPrinciple();
        services.AddProblemDetails();
        services.AddHealthChecks().AddDbContextCheck<CatalogDbContext>("database");
        services.AddCatalogApiVersioning();
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
            options.AddPolicy("product:create", p => p.RequireRole("admin"));
            options.AddPolicy("product:update", p => p.RequireRole("admin"));
            options.AddPolicy("product:delete", p => p.RequireRole("admin"));
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
