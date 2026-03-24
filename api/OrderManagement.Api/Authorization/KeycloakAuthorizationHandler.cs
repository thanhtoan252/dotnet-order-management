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
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        KeycloakPermissionRequirement requirement)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return;
        }

        if (!TryGetBearerToken(httpContext, out var userToken))
        {
            return;
        }

        var cacheKey = BuildCacheKey(context.User, userToken, requirement);
        if (cache.TryGetValue(cacheKey, out bool cached))
        {
            if (cached)
            {
                context.Succeed(requirement);
            }
            return;
        }

        var granted = await CheckPermissionAsync(userToken, requirement);
        cache.Set(cacheKey, granted, TimeSpan.FromSeconds(30));

        if (granted)
        {
            context.Succeed(requirement);
        }
    }

    private static bool TryGetBearerToken(HttpContext httpContext, out string token)
    {
        var header = httpContext.Request.Headers.Authorization.ToString();
        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            token = string.Empty;
            return false;
        }

        token = header["Bearer ".Length..].Trim();
        return true;
    }

    private static string BuildCacheKey(ClaimsPrincipal user, string token, KeycloakPermissionRequirement requirement)
    {
        var sub = user.FindFirstValue("sub")
                  ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? token[^Math.Min(16, token.Length)..];
        return $"kc:{sub}:{requirement.Resource}#{requirement.Scope}";
    }

    private async Task<bool> CheckPermissionAsync(string userToken, KeycloakPermissionRequirement requirement)
    {
        var kc = configuration.GetSection("Keycloak");
        var clientId = kc["ClientId"] ?? throw new InvalidOperationException("Keycloak:ClientId is required");
        var clientSecret = kc["ClientSecret"] ??
                           throw new InvalidOperationException("Keycloak:ClientSecret is required");
        var audience = kc["Audience"] ?? "order-api";
        // TokenEndpoint allows an internal Docker hostname for server-to-server calls
        // while Authority still reflects the public hostname in the JWT issuer claim.
        var tokenEndpoint = kc["TokenEndpoint"]
                            ?? (kc["Authority"] ??
                                throw new InvalidOperationException("Keycloak:Authority is required"))
                            + "/protocol/openid-connect/token";

        var client = httpClientFactory.CreateClient("keycloak");
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

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
        return response.IsSuccessStatusCode;
    }
}
