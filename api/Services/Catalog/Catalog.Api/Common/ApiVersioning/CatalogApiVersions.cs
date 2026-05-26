using Asp.Versioning;

namespace Catalog.Api.Common.ApiVersioning;

internal static class CatalogApiVersions
{
    public const string HeaderName = "X-Api-Version";

    public static readonly ApiVersion V1 = new(1, 0);
}
