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

public sealed class KeycloakCredentialRepresentation
{
    [JsonPropertyName("type")] public string Type { get; set; } = "password";
    [JsonPropertyName("value")] public string Value { get; set; } = string.Empty;
    [JsonPropertyName("temporary")] public bool Temporary { get; set; }
}

public sealed class KeycloakRoleRepresentation
{
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("composite")] public bool? Composite { get; set; }
    [JsonPropertyName("clientRole")] public bool? ClientRole { get; set; }
    [JsonPropertyName("containerId")] public string? ContainerId { get; set; }
}

public sealed class KeycloakTokenResponse
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; } = string.Empty;
    [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
    [JsonPropertyName("token_type")] public string TokenType { get; set; } = "Bearer";
}
