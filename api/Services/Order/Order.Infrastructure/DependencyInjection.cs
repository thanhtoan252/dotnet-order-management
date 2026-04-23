using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Order.Application.Abstractions;
using Order.Application.EventHandlers;
using Order.Application.Services;
using Order.Infrastructure.Data;
using Order.Infrastructure.Outbox;
using Refit;
using Shared.Contracts;
using Shared.Contracts.IntegrationEvents;
using Shared.Messaging;
using Shared.Messaging.Abstractions;

namespace Order.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OrderDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql =>
                {
                    sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
                    sql.CommandTimeout(30);
                    sql.MigrationsAssembly(typeof(OrderDbContext).Assembly.FullName);
                }));

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IOrderDbContext>(sp => sp.GetRequiredService<OrderDbContext>());

        // Outbox pattern (per-service)
        services.AddScoped<IOutboxStore, OutboxStore>();
        services.AddScoped<IIdempotencyStore>(sp => sp.GetRequiredService<IOutboxStore>() as IIdempotencyStore
            ?? throw new InvalidOperationException("OutboxStore must implement IIdempotencyStore"));
        services.AddScoped<IEventBus, OutboxEventBus>();
        services.AddHostedService<OutboxProcessor>();

        // Inventory Service Refit client (sync availability check via API Gateway)
        var gatewayBaseUrl = configuration["ApiGateway:BaseUrl"]
            ?? throw new InvalidOperationException("ApiGateway:BaseUrl is not configured.");
        services.AddRefitClient<IInventoryService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(gatewayBaseUrl))
            .AddStandardResilienceHandler();

        // Kafka messaging
        services.AddMessaging(configuration);

        // Kafka consumers (saga responses from Catalog Service)
        services.AddKafkaConsumer<StockReservedIntegrationEvent, StockReservedConsumer>(Topics.StockReserved);
        services.AddKafkaConsumer<StockReservationFailedIntegrationEvent, StockReservationFailedConsumer>(Topics.StockReservationFailed);

        return services;
    }
}
