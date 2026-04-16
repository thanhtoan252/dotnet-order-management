using System.Threading.RateLimiting;
using ApiGateway.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace ApiGateway.Extensions;

internal static class ServiceExtensions
{
    public static IHostBuilder AddSerilog(this IHostBuilder host)
    {
        return host.UseSerilog((ctx, cfg) => cfg
            .ReadFrom.Configuration(ctx.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"));
    }

    public static IServiceCollection AddKeycloakAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<KeycloakSettings>()
            .BindConfiguration(KeycloakSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var keycloak = configuration.GetSection(KeycloakSettings.SectionName).Get<KeycloakSettings>()
                       ?? throw new InvalidOperationException("Keycloak settings are not configured.");

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
                    ValidIssuer = keycloak.ValidIssuer ?? keycloak.Authority,
                    ValidateAudience = false,
                    RoleClaimType = "roles"
                };
            });

        services.AddAuthorization();

        services.AddHttpClient("keycloak", client =>
        {
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        });

        return services;
    }

    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddFixedWindowLimiter("fixed", opt =>
            {
                opt.PermitLimit = 100;
                opt.Window = TimeSpan.FromMinutes(1);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 10;
            });

            options.AddSlidingWindowLimiter("sliding", opt =>
            {
                opt.PermitLimit = 1000;
                opt.Window = TimeSpan.FromHours(1);
                opt.SegmentsPerWindow = 6;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 10;
            });

            options.AddConcurrencyLimiter("concurrency", opt =>
            {
                opt.PermitLimit = 50;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 25;
            });

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1)
                    }));
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
