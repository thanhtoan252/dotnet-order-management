using OrderManagement.Api.Endpoints;
using Scalar.AspNetCore;
using Serilog;

namespace OrderManagement.Api.Extensions;

internal static class WebApplicationExtensions
{
    public static WebApplication ConfigurePipeline(this WebApplication app)
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
            opts.EnrichDiagnosticContext = (ctx, httpContext) =>
            {
                ctx.Set("RequestHost", httpContext.Request.Host.Value);
                ctx.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString()!);
            };
        });

        app.MapOrderEndpoints();
        app.MapProductEndpoints();
        app.MapHealthChecks("/health").WithTags("Health");

        return app;
    }
}
