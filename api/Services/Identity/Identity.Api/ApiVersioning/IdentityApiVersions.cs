using Asp.Versioning;

namespace Identity.Api.ApiVersioning;

internal static class IdentityApiVersions
{
    public const string HeaderName = "X-Api-Version";

    public static readonly ApiVersion V1 = new(1, 0);
}
