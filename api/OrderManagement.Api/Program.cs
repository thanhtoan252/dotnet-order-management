using OrderManagement.Api.Extensions;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting OrderManagement API...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.AddSerilog();

    builder.Services
        .AddApplicationServices(builder.Configuration)
        .AddKeycloakAuth(builder.Configuration.GetSection("Keycloak"))
        .AddCorsPolicy(builder.Configuration, builder.Environment);

    var app = builder.Build();

    await app.ConfigurePipeline().RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}