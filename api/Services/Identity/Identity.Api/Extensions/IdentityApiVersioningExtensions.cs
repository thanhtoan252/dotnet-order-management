using Asp.Versioning;
using Asp.Versioning.Builder;
using Identity.Api.ApiVersioning;

namespace Identity.Api.Extensions;

internal static class IdentityApiVersioningExtensions
{
    public static IServiceCollection AddIdentityApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = IdentityApiVersions.V1;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new HeaderApiVersionReader(IdentityApiVersions.HeaderName);
        });

        return services;
    }

    public static ApiVersionSet NewIdentityApiVersionSet(this IEndpointRouteBuilder app)
    {
        return app.NewApiVersionSet()
            .HasApiVersion(IdentityApiVersions.V1)
            .ReportApiVersions()
            .Build();
    }
}
