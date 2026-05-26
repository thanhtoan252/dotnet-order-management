namespace Identity.Api.Endpoints.Users.V1.DTOs;

public sealed record UserResponse(
    string Id,
    string Username,
    string? Email,
    string? FirstName,
    string? LastName,
    bool Enabled,
    bool EmailVerified,
    long? CreatedTimestamp,
    IReadOnlyList<string> Roles);
