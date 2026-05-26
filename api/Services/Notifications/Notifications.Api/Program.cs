using Notifications.Api.Endpoints.Notifications.V1;
using Notifications.Api.Extensions;
using Notifications.Application.Templates.Seeding;
using Notifications.Infrastructure.Realtime;
using Scalar.AspNetCore;
using Serilog;
using Shared.Observability;
using Shared.Web.Authentication;
using Shared.Web.Cors;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Notifications API...");

    var builder = WebApplication.CreateBuilder(args);

    builder.AddObservability("notifications-api");
    var keycloakOptions = builder.Configuration.GetRequiredSection(KeycloakJwtOptions.SectionName)
                              .Get<KeycloakJwtOptions>()
                          ?? throw new InvalidOperationException("Keycloak settings are not configured.");
    var corsOptions = builder.Configuration.GetSection(AppCorsOptions.SectionName).Get<AppCorsOptions>()
                      ?? new AppCorsOptions();

    builder.Services
        .AddApplicationServices(builder.Configuration)
        .AddJwtAuth(keycloakOptions)
        .AddCorsPolicy(corsOptions, builder.Environment);

    var app = builder.Build();

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

    app.MapNotificationEndpoints();
    app.MapHub<NotificationHub>("/hubs/notifications");
    app.MapHealthChecks("/health").WithTags("Health");

    await using (var scope = app.Services.CreateAsyncScope())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<TemplateSeeder>();
        await seeder.SeedAsync();
    }

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
