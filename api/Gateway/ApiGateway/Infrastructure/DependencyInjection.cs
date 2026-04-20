using ApiGateway.Infrastructure.Authentication;
using ApiGateway.Infrastructure.Cors;
using ApiGateway.Infrastructure.RateLimiting;
using ApiGateway.Infrastructure.ReverseProxy;

namespace ApiGateway.Infrastructure;

internal static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddYarpReverseProxy(configuration);
        services.AddKeycloakAuthentication(configuration);
        services.AddRateLimiting();
        services.AddCorsPolicy(configuration, environment);
        services.AddHealthChecks();

        return services;
    }
}
