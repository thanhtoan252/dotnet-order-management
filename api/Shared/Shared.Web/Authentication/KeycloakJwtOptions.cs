namespace Shared.Web.Authentication;

public sealed class KeycloakJwtOptions
{
    public const string SectionName = "Keycloak";

    public string Authority { get; init; } = string.Empty;

    public string? ValidIssuer { get; init; }
}
