using ApiGateway.Application.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace ApiGateway.Infrastructure;

internal static class AuthConfiguration
{
    public static IServiceCollection AddKeycloakAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<KeycloakSettings>()
            .BindConfiguration(KeycloakSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var keycloak = configuration.GetSection(KeycloakSettings.SectionName).Get<KeycloakSettings>()
                               ?? throw new InvalidOperationException("Keycloak settings are not configured.");

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
        services.AddInfrastructure(configuration);

        return services;
    }
}
