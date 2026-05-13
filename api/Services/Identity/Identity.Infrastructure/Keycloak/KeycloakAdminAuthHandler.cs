using System.Net.Http.Headers;

namespace Identity.Infrastructure.Keycloak;

public sealed class KeycloakAdminAuthHandler(IKeycloakAdminTokenProvider tokenProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var token = await tokenProvider.GetAccessTokenAsync(ct);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, ct);
    }
}
