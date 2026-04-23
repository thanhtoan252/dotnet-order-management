using Inventory.Application.Abstractions;
using Inventory.Application.EventHandlers;
using Inventory.Infrastructure.Data;
using Inventory.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Contracts;
using Shared.Contracts.IntegrationEvents;
using Shared.Messaging;
using Shared.Messaging.Abstractions;

namespace Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<InventoryDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql =>
                {
                    sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
                    sql.CommandTimeout(30);
                    sql.MigrationsAssembly(typeof(InventoryDbContext).Assembly.FullName);
                }));

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IInventoryDbContext>(sp => sp.GetRequiredService<InventoryDbContext>());

        services.AddScoped<IOutboxStore, OutboxStore>();
        services.AddScoped<IIdempotencyStore>(sp => sp.GetRequiredService<IOutboxStore>() as IIdempotencyStore
            ?? throw new InvalidOperationException("OutboxStore must implement IIdempotencyStore"));
        services.AddScoped<IEventBus, OutboxEventBus>();
        services.AddHostedService<OutboxProcessor>();

        services.AddMessaging(configuration);

        services.AddKafkaConsumer<ProductCreatedIntegrationEvent, ProductCreatedConsumer>(Topics.ProductCreated);
        services.AddKafkaConsumer<ProductDeletedIntegrationEvent, ProductDeletedConsumer>(Topics.ProductDeleted);
        services.AddKafkaConsumer<ProductRenamedIntegrationEvent, ProductRenamedConsumer>(Topics.ProductRenamed);

        services.AddKafkaConsumer<OrderPlacedIntegrationEvent, OrderPlacedConsumer>(Topics.OrderPlaced);
        services.AddKafkaConsumer<OrderCancelledIntegrationEvent, OrderCancelledConsumer>(Topics.OrderCancelled);

        return services;
    }
}
