namespace Identity.Application.Users.Models;

public sealed record UpdateUserInput(
    string? Email,
    string? FirstName,
    string? LastName,
    bool Enabled);
