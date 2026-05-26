using Asp.Versioning;

namespace Inventory.Api.ApiVersioning;

internal static class InventoryApiVersions
{
    public const string HeaderName = "X-Api-Version";

    public static readonly ApiVersion V1 = new(1, 0);
}
