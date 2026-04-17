using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Order.Infrastructure.Data;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Order migration runner starting...");

    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddDbContext<OrderDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration["ConnectionStrings:DefaultConnection"],
            sql =>
            {
                sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
                sql.CommandTimeout(30);
                sql.MigrationsAssembly(typeof(OrderDbContext).Assembly.FullName);
            }));
    builder.Services.AddSingleton(TimeProvider.System);

    using var host = builder.Build();

    await using var scope = host.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

    Log.Information("Applying pending migrations...");
    await db.Database.MigrateAsync();
    Log.Information("Order migrations applied successfully.");

    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Order migration runner failed.");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}
