using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Notifications.Application;
using Notifications.Infrastructure;
using Notifications.Infrastructure.Data;
using Shared.Web.Authentication;
using Shared.Web.Cors;
using Shared.Web.Extensions;
using Shared.Web.Middleware;

namespace Notifications.Api.Extensions;

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
        services.AddHealthChecks().AddDbContextCheck<NotificationsDbContext>("database");
        services.AddNotificationsApiVersioning();
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

                // SignalR WebSocket negotiation can't send Authorization headers in browsers;
                // accept the bearer token from the ?access_token= query string for /hubs/.
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("notifications:admin", p => p.RequireRole("admin"));
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
                options.AddPolicy("CorsPolicy",
                    p => p.SetIsOriginAllowed(_ => true).AllowAnyMethod().AllowAnyHeader().AllowCredentials());
            }
            else
            {
                options.AddPolicy("CorsPolicy",
                    p => p.WithOrigins(corsOptions.AllowedOrigins).AllowAnyMethod().AllowAnyHeader().AllowCredentials());
            }
        });

        return services;
    }
}
