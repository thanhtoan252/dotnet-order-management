namespace ApiGateway.Models;

internal sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    string Username,
    string[] Roles,
    int ExpiresIn);
