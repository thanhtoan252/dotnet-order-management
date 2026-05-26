using Asp.Versioning;

namespace Order.Api.ApiVersioning;

internal static class OrderApiVersions
{
    public const string HeaderName = "X-Api-Version";

    public static readonly ApiVersion V1 = new(1, 0);
}
