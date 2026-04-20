using ApiGateway.Application;
using ApiGateway.Application.Endpoints;
using ApiGateway.Infrastructure;
using ApiGateway.Infrastructure.Cors;
using ApiGateway.Infrastructure.Logging;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting API Gateway...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.AddSerilog();

    builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
    builder.Services.AddApplication();

    var app = builder.Build();

    app.UseCors(CorsExtensions.PolicyName);
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseSerilogRequestLogging(opts =>
    {
        opts.MessageTemplate =
            "Gateway: {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
    });

    app.MapAuthEndpoints();
    app.MapReverseProxy();
    app.MapHealthChecks("/health").WithTags("Health");

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "API Gateway terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
