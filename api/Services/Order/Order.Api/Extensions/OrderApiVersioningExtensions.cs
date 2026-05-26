using Asp.Versioning;
using Asp.Versioning.Builder;
using Order.Api.ApiVersioning;

namespace Order.Api.Extensions;

internal static class OrderApiVersioningExtensions
{
    public static IServiceCollection AddOrderApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = OrderApiVersions.V1;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new HeaderApiVersionReader(OrderApiVersions.HeaderName);
        });

        return services;
    }

    public static ApiVersionSet NewOrderApiVersionSet(this IEndpointRouteBuilder app)
    {
        return app.NewApiVersionSet()
            .HasApiVersion(OrderApiVersions.V1)
            .ReportApiVersions()
            .Build();
    }
}
