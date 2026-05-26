namespace Identity.Api.Endpoints.Users.V1.DTOs;

public sealed record UpdateUserRequest(
    string? Email,
    string? FirstName,
    string? LastName,
    bool Enabled);
