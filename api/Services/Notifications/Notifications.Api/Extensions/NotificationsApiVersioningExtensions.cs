using Asp.Versioning;
using Asp.Versioning.Builder;
using Notifications.Api.ApiVersioning;

namespace Notifications.Api.Extensions;

internal static class NotificationsApiVersioningExtensions
{
    public static IServiceCollection AddNotificationsApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = NotificationsApiVersions.V1;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new HeaderApiVersionReader(NotificationsApiVersions.HeaderName);
        });

        return services;
    }

    public static ApiVersionSet NewNotificationsApiVersionSet(this IEndpointRouteBuilder app)
    {
        return app.NewApiVersionSet()
            .HasApiVersion(NotificationsApiVersions.V1)
            .ReportApiVersions()
            .Build();
    }
}
