using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Order.Application;
using Order.Infrastructure;
using Order.Infrastructure.Data;
using Serilog;
using Serilog.Formatting.Json;
using Shared.Web.Middleware;

namespace Order.Api.Extensions;

internal static class ServiceExtensions
{
    public static IHostBuilder AddSerilog(this IHostBuilder host)
    {
        return host.UseSerilog((ctx, services, cfg) => cfg
            .ReadFrom.Configuration(ctx.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithThreadId()
            .Enrich.WithProcessId()
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.File(
                path: "logs/order-service-.json",
                formatter: new JsonFormatter(),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7));
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddApplication()
            .AddInfrastructure(configuration);

        services.AddValidatorsFromAssemblyContaining<Program>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddHealthChecks().AddDbContextCheck<OrderDbContext>("database");
        services.AddOpenApi();

        return services;
    }

    public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfigurationSection keycloak)
    {
        var authority = keycloak["Authority"]!;
        var validIssuer = keycloak["ValidIssuer"] ?? authority;

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
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

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            if (environment.IsDevelopment() || allowedOrigins.Length == 0)
            {
                options.AddPolicy("CorsPolicy", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            }
            else
            {
                options.AddPolicy("CorsPolicy", p => p.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader());
            }
        });

        return services;
    }
}
