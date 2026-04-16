using System.IdentityModel.Tokens.Jwt;
using ApiGateway.Application.Contracts;
using ApiGateway.Application.Options;
using ApiGateway.Application.Services;
using Microsoft.Extensions.Options;
using Refit;

namespace ApiGateway.EndPoints;

internal static class AuthEndpoints
{
    public static WebApplication MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/api/auth/login", async (
            LoginRequest request,
            IOptions<KeycloakSettings> keycloakOptions,
            IKeycloakClient keycloakClient,
            ILogger<Program> logger) =>
        {
            var keycloak = keycloakOptions.Value;

            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = keycloak.ClientId,
                ["username"] = request.Username,
                ["password"] = request.Password
            };

            try
            {
                var token = await keycloakClient.GetTokenAsync(form);
                var (username, roles) = ExtractClaims(token.AccessToken, request.Username);

                return Results.Ok(new LoginResponse(token.AccessToken, token.RefreshToken, username, roles, token.ExpiresIn));
            }
            catch (ApiException ex)
            {
                logger.LogWarning("Keycloak authentication failed: {StatusCode} {Error}",
                    (int)ex.StatusCode, ex.Content);

                return Results.Unauthorized();
            }
        })
        .AllowAnonymous()
        .RequireCors("CorsPolicy")
        .WithTags("Auth");

        return app;
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
