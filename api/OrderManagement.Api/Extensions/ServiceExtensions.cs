using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using OrderManagement.Api.Authorization;
using OrderManagement.Api.Middleware;
using OrderManagement.Application;
using OrderManagement.Infrastructure;
using OrderManagement.Infrastructure.Data;
using Serilog;
using Serilog.Formatting.Json;

namespace OrderManagement.Api.Extensions;

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
                path: "logs/order-management-.json",
                formatter: new JsonFormatter(),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7));
    }

    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services, IConfiguration configuration)
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

    public static IServiceCollection AddKeycloakAuth(
        this IServiceCollection services, IConfigurationSection kc)
    {
        var authority = kc["Authority"] ?? "";
        var metadataAddress = kc["MetadataAddress"] ?? authority + "/.well-known/openid-configuration";

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.MetadataAddress = metadataAddress;
                options.RequireHttpsMetadata = false;
                options.RefreshOnIssuerKeyNotFound = true;
                options.BackchannelHttpHandler = BuildBackchannelHandler(authority, metadataAddress);
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidAudience = kc["Audience"] ?? "order-api",
                    ValidateIssuer = true,
                    ValidIssuer = authority,
                    RoleClaimType = "roles"
                };
            });

        services.AddHttpContextAccessor();
        services.AddMemoryCache();
        services.AddHttpClient("keycloak");
        services.AddScoped<IAuthorizationHandler, KeycloakAuthorizationHandler>();

        services.AddAuthorization(options =>
        {
            static void AddPermission(AuthorizationOptions o, string scope, string resource)
            {
                o.AddPolicy(scope, p => p.AddRequirements(new KeycloakPermissionRequirement(resource, scope)));
            }

            AddPermission(options, "order:confirm", "Order Resource");
            AddPermission(options, "order:ship", "Order Resource");
            AddPermission(options, "order:deliver", "Order Resource");
            AddPermission(options, "order:delete", "Order Resource");

            AddPermission(options, "product:create", "Product Resource");
            AddPermission(options, "product:update", "Product Resource");
            AddPermission(options, "product:delete", "Product Resource");
        });

        return services;
    }

    // Builds an HttpClientHandler that rewrites the public Keycloak URL to the internal
    // Docker hostname for all JwtBearer backchannel requests (JWKS fetch, metadata).
    // Needed because the discovery document's jwks_uri uses the public hostname
    // (e.g. localhost:8180) which is unreachable from inside the API container.
    private static HttpMessageHandler BuildBackchannelHandler(string authority, string metadataAddress)
    {
        var publicBase = GetBaseUrl(authority);
        var internalBase = GetBaseUrl(metadataAddress);

        if (string.IsNullOrEmpty(publicBase) || publicBase == internalBase)
        {
            return new HttpClientHandler();
        }

        return new RewriteUrlHandler(publicBase, internalBase);
    }

    private static string GetBaseUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return string.Empty;
        }

        return $"{uri.Scheme}://{uri.Host}:{uri.Port}";
    }

    public static IServiceCollection AddCorsPolicy(
        this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
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

    private sealed class RewriteUrlHandler(string from, string to) : HttpClientHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.RequestUri is not null)
            {
                var original = request.RequestUri.ToString();
                if (original.StartsWith(from, StringComparison.OrdinalIgnoreCase))
                {
                    request.RequestUri = new Uri(to + original[from.Length..]);
                }
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
