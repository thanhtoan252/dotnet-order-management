namespace Identity.Application.Users.Models;

public sealed record AssignRolesRequest(IReadOnlyList<string> Roles);
