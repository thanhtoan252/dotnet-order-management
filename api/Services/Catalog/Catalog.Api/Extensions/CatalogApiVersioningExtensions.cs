using Asp.Versioning;
using Asp.Versioning.Builder;
using Catalog.Api.Common.ApiVersioning;

namespace Catalog.Api.Extensions;

internal static class CatalogApiVersioningExtensions
{
    public static IServiceCollection AddCatalogApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = CatalogApiVersions.V1;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new HeaderApiVersionReader(CatalogApiVersions.HeaderName);
        });

        return services;
    }

    public static ApiVersionSet NewCatalogApiVersionSet(this IEndpointRouteBuilder app)
    {
        return app.NewApiVersionSet()
            .HasApiVersion(CatalogApiVersions.V1)
            .ReportApiVersions()
            .Build();
    }
}
