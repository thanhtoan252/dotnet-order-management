using ApiGateway.Infrastructure;
using ApiGateway.EndPoints;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting API Gateway...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.AddSerilog();

    builder.Services
        .AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

    builder.Services.AddKeycloakAuth(builder.Configuration);
    builder.Services.AddRateLimiting();
    builder.Services.AddCorsPolicy(builder.Configuration, builder.Environment);
    builder.Services.AddHealthChecks();

    var app = builder.Build();

    app.UseCors("CorsPolicy");
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
