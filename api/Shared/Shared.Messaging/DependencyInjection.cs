using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Contracts.IntegrationEvents;
using Shared.Messaging.Abstractions;
using Shared.Messaging.Kafka;

namespace Shared.Messaging;

public static class DependencyInjection
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));
        services.AddSingleton<KafkaProducer>();

        return services;
    }

    public static IServiceCollection AddKafkaConsumer<TEvent, THandler>(this IServiceCollection services, string topic)
        where TEvent : IIntegrationEvent
        where THandler : class, IEventConsumer<TEvent>
    {
        services.AddScoped<IEventConsumer<TEvent>, THandler>();
        services.AddHostedService(sp =>
        {
            var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<KafkaOptions>>();
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<KafkaConsumerHost<TEvent>>>();
            return new KafkaConsumerHost<TEvent>(topic, scopeFactory, options, logger);
        });

        return services;
    }
}
