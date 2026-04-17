using ApiGateway.Application.Contracts;
using Refit;

namespace ApiGateway.Application.Services;

public interface IKeycloakClient
{
    [Post("/protocol/openid-connect/token")]
    Task<KeycloakTokenResponse> GetTokenAsync([Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, string> form);
}
