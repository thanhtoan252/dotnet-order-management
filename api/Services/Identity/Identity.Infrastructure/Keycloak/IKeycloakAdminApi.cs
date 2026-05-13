using Refit;

namespace Identity.Infrastructure.Keycloak;

public interface IKeycloakAdminApi
{
    [Get("/admin/realms/{realm}/users")]
    Task<List<KeycloakUserRepresentation>> SearchUsersAsync(
        string realm,
        [Query] string? search,
        [Query] int? first,
        [Query] int? max,
        [Query] bool? enabled,
        [Query] bool briefRepresentation,
        CancellationToken ct);

    [Get("/admin/realms/{realm}/users/count")]
    Task<int> CountUsersAsync(string realm, [Query] string? search, CancellationToken ct);

    [Get("/admin/realms/{realm}/users/{id}")]
    Task<KeycloakUserRepresentation?> GetUserAsync(string realm, string id, CancellationToken ct);

    [Post("/admin/realms/{realm}/users")]
    Task<HttpResponseMessage> CreateUserAsync(string realm, [Body] KeycloakUserRepresentation user, CancellationToken ct);

    [Put("/admin/realms/{realm}/users/{id}")]
    Task UpdateUserAsync(string realm, string id, [Body] KeycloakUserRepresentation user, CancellationToken ct);

    [Delete("/admin/realms/{realm}/users/{id}")]
    Task DeleteUserAsync(string realm, string id, CancellationToken ct);

    [Put("/admin/realms/{realm}/users/{id}/reset-password")]
    Task ResetPasswordAsync(string realm, string id, [Body] KeycloakCredentialRepresentation credential, CancellationToken ct);

    [Get("/admin/realms/{realm}/roles")]
    Task<List<KeycloakRoleRepresentation>> GetRealmRolesAsync(string realm, CancellationToken ct);

    [Get("/admin/realms/{realm}/users/{id}/role-mappings/realm")]
    Task<List<KeycloakRoleRepresentation>> GetUserRealmRolesAsync(string realm, string id, CancellationToken ct);

    [Post("/admin/realms/{realm}/users/{id}/role-mappings/realm")]
    Task AssignUserRealmRolesAsync(string realm, string id, [Body] List<KeycloakRoleRepresentation> roles, CancellationToken ct);

    [Delete("/admin/realms/{realm}/users/{id}/role-mappings/realm")]
    Task RemoveUserRealmRolesAsync(string realm, string id, [Body] List<KeycloakRoleRepresentation> roles, CancellationToken ct);
}

public interface IKeycloakTokenApi
{
    [Post("/realms/{realm}/protocol/openid-connect/token")]
    Task<KeycloakTokenResponse> GetTokenAsync(
        string realm,
        [Body(BodySerializationMethod.UrlEncoded)] IDictionary<string, string> form,
        CancellationToken ct);
}
