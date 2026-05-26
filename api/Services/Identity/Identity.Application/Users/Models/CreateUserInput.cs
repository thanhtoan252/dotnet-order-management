namespace Identity.Application.Users.Models;

public sealed record CreateUserInput(
    string Username,
    string? Email,
    string? FirstName,
    string? LastName,
    bool Enabled,
    string Password,
    bool TemporaryPassword,
    IReadOnlyList<string> Roles);
