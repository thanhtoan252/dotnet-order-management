using System.ComponentModel.DataAnnotations;

namespace ApiGateway.Infrastructure.Authentication;

public sealed class KeycloakSettings
{
    public const string SectionName = "Keycloak";

    [Required]
    public string Authority { get; init; } = null!;

    public string? ValidIssuer { get; init; }

    [Required]
    public string ClientId { get; init; } = null!;

    public string TokenEndpoint => $"{Authority}/protocol/openid-connect/token";
}
