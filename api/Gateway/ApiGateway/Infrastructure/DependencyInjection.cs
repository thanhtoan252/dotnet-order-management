using ApiGateway.Application.Options;
using ApiGateway.Application.Services;
using Refit;

namespace ApiGateway.Infrastructure;

internal static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var keycloak = configuration.GetSection(KeycloakSettings.SectionName).Get<KeycloakSettings>()
                       ?? throw new InvalidOperationException("Keycloak settings are not configured.");

        services.AddRefitClient<IKeycloakClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(keycloak.Authority))
            .AddStandardResilienceHandler();

        return services;
    }
}
