using Refit;

namespace Identity.Infrastructure.Keycloak;

public interface IKeycloakTokenApi
{
    [Post("/realms/{realm}/protocol/openid-connect/token")]
    Task<KeycloakTokenResponse> GetTokenAsync(
        string realm,
        [Body(BodySerializationMethod.UrlEncoded)] IDictionary<string, string> form,
        CancellationToken ct);
}
