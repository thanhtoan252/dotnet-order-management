using ApiGateway.Infrastructure.Authentication;
using ApiGateway.Infrastructure.Cors;
using ApiGateway.Infrastructure.RateLimiting;
using ApiGateway.Infrastructure.ReverseProxy;

namespace ApiGateway.Infrastructure;

internal static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var corsOptions = configuration.GetSection(GatewayCorsOptions.SectionName).Get<GatewayCorsOptions>()
                          ?? new GatewayCorsOptions();

        services.AddYarpReverseProxy(configuration);
        services.AddKeycloakAuthentication(configuration);
        services.AddRateLimiting();
        services.AddCorsPolicy(corsOptions, environment);
        services.AddHealthChecks();

        return services;
    }
}
