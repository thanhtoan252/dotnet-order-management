using Refit;

namespace ApiGateway.Infrastructure.Authentication;

public interface IKeycloakClient
{
    [Post("/protocol/openid-connect/token")]
    Task<KeycloakTokenResponse> GetTokenAsync([Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, string> form);
}
