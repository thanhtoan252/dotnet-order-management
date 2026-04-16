namespace ApiGateway.Application.Contracts;

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    string Username,
    string[] Roles,
    int ExpiresIn
);
