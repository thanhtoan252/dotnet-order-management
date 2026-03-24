using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;

namespace OrderManagement.Api.Authorization;

/// <summary>
///     Calls Keycloak's UMA token endpoint with response_mode=decision to check whether
///     the current user holds the required resource+scope permission.
///     Results are cached for 30 s to avoid a round-trip on every request.
/// </summary>
public class KeycloakAuthorizationHandler(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    IHttpContextAccessor httpContextAccessor,
    IMemoryCache cache) : AuthorizationHandler<KeycloakPermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        KeycloakPermissionRequirement requirement)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null) return;

        // Extract Bearer token from the incoming request
        var authHeader = httpContext.Request.Headers.Authorization.ToString();
        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return;
        var userToken = authHeader["Bearer ".Length..].Trim();

        // Cache key: stable per user × permission
        var sub = context.User.FindFirstValue("sub")
                  ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? userToken[^Math.Min(16, userToken.Length)..];
        var cacheKey = $"kc:{sub}:{requirement.Resource}#{requirement.Scope}";

        if (cache.TryGetValue(cacheKey, out bool granted))
        {
            if (granted) context.Succeed(requirement);
            return;
        }

        var kc = configuration.GetSection("Keycloak");
        var clientId = kc["ClientId"] ?? throw new InvalidOperationException("Keycloak:ClientId is required");
        var clientSecret = kc["ClientSecret"] ??
                           throw new InvalidOperationException("Keycloak:ClientSecret is required");
        var audience = kc["Audience"] ?? "order-api";
        // TokenEndpoint allows an internal Docker hostname to be used for server-to-server
        // calls while Authority still reflects the public hostname in the JWT issuer claim.
        var tokenEndpoint = kc["TokenEndpoint"]
                            ?? (kc["Authority"] ??
                                throw new InvalidOperationException("Keycloak:Authority is required"))
                            + "/protocol/openid-connect/token";

        var client = httpClientFactory.CreateClient("keycloak");
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        // UMA grant: ask Keycloak whether subject_token holds the requested permission.
        // response_mode=decision returns {"result":true} instead of a full RPT token.
        var body = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "urn:ietf:params:oauth:grant-type:uma-ticket",
            ["audience"] = audience,
            ["permission"] = $"{requirement.Resource}#{requirement.Scope}",
            ["subject_token"] = userToken,
            ["response_mode"] = "decision"
        });

        var response = await client.PostAsync(tokenEndpoint, body);

        granted = response.IsSuccessStatusCode;
        cache.Set(cacheKey, granted, TimeSpan.FromSeconds(30));

        if (granted) context.Succeed(requirement);
    }
}