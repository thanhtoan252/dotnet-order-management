using Microsoft.EntityFrameworkCore;
using OrderManagement.Api.Endpoints;
using OrderManagement.Infrastructure.Data;
using Scalar.AspNetCore;
using Serilog;

namespace OrderManagement.Api.Extensions;

internal static class WebApplicationExtensions
{
    public static async Task<WebApplication> ConfigurePipelineAsync(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            await using var scope = app.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
            await db.Database.MigrateAsync();
        }

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