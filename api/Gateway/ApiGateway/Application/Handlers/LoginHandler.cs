using System.IdentityModel.Tokens.Jwt;
using ApiGateway.Application.Contracts;
using ApiGateway.Infrastructure.Authentication;
using Microsoft.Extensions.Options;
using Refit;

namespace ApiGateway.Application.Handlers;

public sealed class LoginHandler(
    IKeycloakClient keycloakClient,
    IOptions<KeycloakSettings> keycloakOptions,
    ILogger<LoginHandler> logger)
{
    private readonly KeycloakSettings _keycloakSettings = keycloakOptions.Value;

    public async Task<LoginResponse?> HandleAsync(LoginRequest request, CancellationToken ct)
    {
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = _keycloakSettings.ClientId,
            ["username"] = request.Username,
            ["password"] = request.Password
        };

        try
        {
            var token = await keycloakClient.GetTokenAsync(form);
            var (username, roles) = ExtractClaims(token.AccessToken, request.Username);

            return new LoginResponse(token.AccessToken, token.RefreshToken, username, roles, token.ExpiresIn);
        }
        catch (ApiException ex)
        {
            logger.LogWarning("Keycloak authentication failed: {StatusCode} {Error}", (int)ex.StatusCode, ex.Content);

            return null;
        }
    }

    private static (string Username, string[] Roles) ExtractClaims(string accessToken, string fallbackUsername)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(accessToken);

        var username = jwt.Claims
            .FirstOrDefault(c => c.Type == "preferred_username")?.Value ?? fallbackUsername;

        var roles = jwt.Claims
            .Where(c => c.Type == "roles")
            .Select(c => c.Value)
            .ToArray();

        return (username, roles);
    }
}
