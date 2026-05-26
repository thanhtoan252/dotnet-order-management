namespace Identity.Api.Endpoints.Users.V1.DTOs;

public sealed record CreateUserRequest(
    string Username,
    string? Email,
    string? FirstName,
    string? LastName,
    bool Enabled,
    string Password,
    bool TemporaryPassword,
    IReadOnlyList<string> Roles);
