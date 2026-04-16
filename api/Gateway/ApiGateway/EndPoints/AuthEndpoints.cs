using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using ApiGateway.Models;
using ApiGateway.Options;
using Microsoft.Extensions.Options;

namespace ApiGateway.EndPoints;

internal static class AuthEndpoints
{
    public static WebApplication MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/api/auth/login", async (
            LoginRequest request,
            IOptions<KeycloakSettings> keycloakOptions,
            IHttpClientFactory httpClientFactory,
            ILogger<Program> logger) =>
        {
            var keycloak = keycloakOptions.Value;
            var httpClient = httpClientFactory.CreateClient("keycloak");

            var formData = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = keycloak.ClientId,
                ["username"] = request.Username,
                ["password"] = request.Password
            });

            using var response = await httpClient.PostAsync(keycloak.TokenEndpoint, formData);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                logger.LogWarning("Keycloak authentication failed: {StatusCode} {Error}",
                    (int)response.StatusCode, errorBody);

                return Results.Unauthorized();
            }

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            var accessToken = json.GetProperty("access_token").GetString()!;
            var refreshToken = json.GetProperty("refresh_token").GetString()!;
            var expiresIn = json.GetProperty("expires_in").GetInt32();

            var (username, roles) = ExtractClaims(accessToken, request.Username);

            return Results.Ok(new LoginResponse(accessToken, refreshToken, username, roles, expiresIn));
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
