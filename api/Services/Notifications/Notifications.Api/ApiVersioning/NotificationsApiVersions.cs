using Asp.Versioning;

namespace Notifications.Api.ApiVersioning;

internal static class NotificationsApiVersions
{
    public const string HeaderName = "X-Api-Version";

    public static readonly ApiVersion V1 = new(1, 0);
}
