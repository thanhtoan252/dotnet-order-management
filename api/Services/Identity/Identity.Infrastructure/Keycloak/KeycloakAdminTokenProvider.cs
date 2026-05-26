using Microsoft.Extensions.Options;

namespace Identity.Infrastructure.Keycloak;

public sealed class KeycloakAdminTokenProvider(
    IKeycloakTokenApi tokenApi,
    IOptions<KeycloakOptions> options,
    TimeProvider time)
    : IKeycloakAdminTokenProvider
{
    private static readonly TimeSpan SafetyMargin = TimeSpan.FromSeconds(30);
    private readonly KeycloakOptions _options = options.Value;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private string? _cachedToken;
    private DateTimeOffset _cachedExpiresAt;

    public async Task<string> GetAccessTokenAsync(CancellationToken ct)
    {
        var now = time.GetUtcNow();
        if (_cachedToken is not null && now + SafetyMargin < _cachedExpiresAt)
        {
            return _cachedToken;
        }

        await _gate.WaitAsync(ct);
        try
        {
            now = time.GetUtcNow();
            if (_cachedToken is not null && now + SafetyMargin < _cachedExpiresAt)
            {
                return _cachedToken;
            }

            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _options.AdminClientId,
                ["client_secret"] = _options.AdminClientSecret
            };

            var response = await tokenApi.GetTokenAsync(_options.Realm, form, ct);
            _cachedToken = response.AccessToken;
            _cachedExpiresAt = time.GetUtcNow().AddSeconds(response.ExpiresIn);

            return _cachedToken;
        }
        finally
        {
            _gate.Release();
        }
    }
}
