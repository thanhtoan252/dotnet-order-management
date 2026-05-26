namespace Identity.Application.Users.Models;

public sealed record User(
    string Id,
    string Username,
    string? Email,
    string? FirstName,
    string? LastName,
    bool Enabled,
    bool EmailVerified,
    long? CreatedTimestamp,
    IReadOnlyList<string> Roles);
