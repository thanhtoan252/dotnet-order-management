using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Refit;

namespace ApiGateway.Infrastructure.Authentication;

internal static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddKeycloakAuthentication(this IServiceCollection services, IConfiguration configuration)
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

        services.AddRefitClient<IKeycloakClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(keycloak.Authority))
            .AddStandardResilienceHandler();

        return services;
    }
}
