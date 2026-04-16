namespace ApiGateway.Application.Contracts;

public sealed record LoginRequest(
    string Username,
    string Password
);
