namespace Identity.Infrastructure.Keycloak;

public interface IKeycloakAdminTokenProvider
{
    Task<string> GetAccessTokenAsync(CancellationToken ct);
}
