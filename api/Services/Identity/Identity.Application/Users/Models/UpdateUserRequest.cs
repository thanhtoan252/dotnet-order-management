namespace Identity.Application.Users.Models;

public sealed record UpdateUserRequest(
    string? Email,
    string? FirstName,
    string? LastName,
    bool Enabled);
