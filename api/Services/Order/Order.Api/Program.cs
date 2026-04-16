using Order.Api.Extensions;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Order API...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.AddSerilog();

    builder.Services
        .AddApplicationServices(builder.Configuration)
        .AddJwtAuth(builder.Configuration.GetSection("Keycloak"))
        .AddCorsPolicy(builder.Configuration, builder.Environment);

    var app = builder.Build();

    await (await app.ConfigurePipelineAsync()).RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
