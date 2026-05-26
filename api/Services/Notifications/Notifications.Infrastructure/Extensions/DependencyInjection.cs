using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Application.Abstractions;
using Notifications.Application.EventHandlers;
using Notifications.Application.Realtime;
using Notifications.Infrastructure.Data;
using Notifications.Infrastructure.Persistence;
using Notifications.Infrastructure.Realtime;
using Shared.Messaging;
using Shared.Messaging.Abstractions;

namespace Notifications.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<NotificationsDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql =>
                {
                    sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
                    sql.CommandTimeout(30);
                    sql.MigrationsAssembly(typeof(NotificationsDbContext).Assembly.FullName);
                }));

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<INotificationDbContext>(sp => sp.GetRequiredService<NotificationsDbContext>());

        // Idempotency store for KafkaConsumerHost
        services.AddScoped<IIdempotencyStore, ProcessedMessageStore>();

        // SignalR hub + pusher
        services.AddSignalR();
        services.AddSingleton<IUserIdProvider, SubClaimUserIdProvider>();
        services.AddScoped<INotificationPusher, SignalRNotificationPusher>();

        // Kafka messaging
        services.AddMessaging(configuration);

        // Kafka consumers — all 7 event types
        services.AddKafkaConsumer<OrderPlacedConsumer>();
        services.AddKafkaConsumer<OrderCancelledConsumer>();
        services.AddKafkaConsumer<StockReservedConsumer>();
        services.AddKafkaConsumer<StockReservationFailedConsumer>();
        services.AddKafkaConsumer<ProductCreatedConsumer>();
        services.AddKafkaConsumer<ProductRenamedConsumer>();
        services.AddKafkaConsumer<ProductDeletedConsumer>();

        return services;
    }
}
