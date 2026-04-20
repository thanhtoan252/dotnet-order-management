using Inventory.Api.Endpoints;
using Scalar.AspNetCore;
using Serilog;

namespace Inventory.Api.Extensions;

internal static class WebApplicationExtensions
{
    public static async Task<WebApplication> ConfigurePipelineAsync(this WebApplication app)
    {
        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseCors("CorsPolicy");
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseSerilogRequestLogging(opts =>
        {
            opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
        });

        app.MapInventoryEndpoints();
        app.MapInventoryInternalEndpoints();
        app.MapHealthChecks("/health").WithTags("Health");

        return app;
    }
}
