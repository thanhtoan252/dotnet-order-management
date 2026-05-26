using System.Text.Json.Serialization;

namespace Identity.Infrastructure.Keycloak;

public sealed class KeycloakTokenResponse
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; } = string.Empty;
    [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
    [JsonPropertyName("token_type")] public string TokenType { get; set; } = "Bearer";
}
