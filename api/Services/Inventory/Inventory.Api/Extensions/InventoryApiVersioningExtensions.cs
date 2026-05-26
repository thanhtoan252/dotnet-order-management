using Asp.Versioning;
using Asp.Versioning.Builder;
using Inventory.Api.ApiVersioning;

namespace Inventory.Api.Extensions;

internal static class InventoryApiVersioningExtensions
{
    public static IServiceCollection AddInventoryApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = InventoryApiVersions.V1;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new HeaderApiVersionReader(InventoryApiVersions.HeaderName);
        });

        return services;
    }

    public static ApiVersionSet NewInventoryApiVersionSet(this IEndpointRouteBuilder app)
    {
        return app.NewApiVersionSet()
            .HasApiVersion(InventoryApiVersions.V1)
            .ReportApiVersions()
            .Build();
    }
}
