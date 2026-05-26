namespace Identity.Application.Users.Models;

public sealed record AssignRolesInput(IReadOnlyList<string> Roles);
