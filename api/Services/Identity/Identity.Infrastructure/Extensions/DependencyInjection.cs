using Identity.Application.Users.Abstractions;
using Identity.Infrastructure.Keycloak;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);

        services
            .AddOptions<KeycloakOptions>()
            .Bind(configuration.GetSection(KeycloakOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.AdminBaseUrl), "Keycloak:AdminBaseUrl is required.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Realm), "Keycloak:Realm is required.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.AdminClientId), "Keycloak:AdminClientId is required.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.AdminClientSecret), "Keycloak:AdminClientSecret is required.")
            .ValidateOnStart();

        var section = configuration.GetSection(KeycloakOptions.SectionName);
        var adminBaseUrl = section["AdminBaseUrl"]
            ?? throw new InvalidOperationException("Keycloak:AdminBaseUrl is required.");

        services.AddSingleton<IKeycloakAdminTokenProvider, KeycloakAdminTokenProvider>();
        services.AddTransient<KeycloakAdminAuthHandler>();

        services.AddRefitClient<IKeycloakTokenApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(adminBaseUrl))
            .AddStandardResilienceHandler();

        services.AddRefitClient<IKeycloakAdminApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(adminBaseUrl))
            .AddHttpMessageHandler<KeycloakAdminAuthHandler>()
            .AddStandardResilienceHandler();

        services.AddScoped<IKeycloakUserService, KeycloakUserService>();

        return services;
    }
}
