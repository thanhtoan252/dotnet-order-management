namespace Identity.Api.Endpoints.Users.V1.DTOs;

public sealed record AssignRolesRequest(IReadOnlyList<string> Roles);
