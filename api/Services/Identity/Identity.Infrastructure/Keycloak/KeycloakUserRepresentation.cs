using System.Text.Json.Serialization;

namespace Identity.Infrastructure.Keycloak;

public sealed class KeycloakUserRepresentation
{
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("username")] public string? Username { get; set; }
    [JsonPropertyName("email")] public string? Email { get; set; }
    [JsonPropertyName("firstName")] public string? FirstName { get; set; }
    [JsonPropertyName("lastName")] public string? LastName { get; set; }
    [JsonPropertyName("enabled")] public bool? Enabled { get; set; }
    [JsonPropertyName("emailVerified")] public bool? EmailVerified { get; set; }
    [JsonPropertyName("createdTimestamp")] public long? CreatedTimestamp { get; set; }
    [JsonPropertyName("credentials")] public List<KeycloakCredentialRepresentation>? Credentials { get; set; }
}
