using Catalog.Domain.Repositories;
using Catalog.Infrastructure.Data;
using Catalog.Infrastructure.Outbox;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Core.CQRS;
using Shared.Messaging;
using Shared.Messaging.Abstractions;

namespace Catalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CatalogDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql =>
                {
                    sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
                    sql.CommandTimeout(30);
                    sql.MigrationsAssembly(typeof(CatalogDbContext).Assembly.FullName);
                }));

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Outbox pattern (per-service)
        services.AddScoped<IOutboxStore, OutboxStore>();
        services.AddScoped<IIdempotencyStore>(sp => sp.GetRequiredService<IOutboxStore>() as IIdempotencyStore
            ?? throw new InvalidOperationException("OutboxStore must implement IIdempotencyStore"));
        services.AddScoped<IEventBus, OutboxEventBus>();
        services.AddHostedService<OutboxProcessor>();

        // Kafka messaging
        services.AddMessaging(configuration);

        return services;
    }
}
