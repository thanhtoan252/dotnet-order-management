using Microsoft.AspNetCore.Authorization;

namespace OrderManagement.Api.Authorization;

/// <summary>
///     Authorization requirement that delegates the decision to Keycloak Authorization Services.
/// </summary>
/// <param name="Resource">Resource name as defined in Keycloak (e.g. "Order Resource").</param>
/// <param name="Scope">Scope name as defined in Keycloak (e.g. "order:confirm").</param>
public record KeycloakPermissionRequirement(string Resource, string Scope) : IAuthorizationRequirement;