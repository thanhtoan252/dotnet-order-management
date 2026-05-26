using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

    public static IServiceCollection AddKafkaConsumer<THandler>(this IServiceCollection services)
        where THandler : class
    {
        var eventType = typeof(THandler).GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventConsumer<>))
            ?.GetGenericArguments()[0]
            ?? throw new InvalidOperationException(
                $"{typeof(THandler).Name} must implement IEventConsumer<TEvent>.");

        return (IServiceCollection)RegisterCoreMethod
            .MakeGenericMethod(eventType, typeof(THandler))
            .Invoke(null, [services])!;
    }

    private static readonly MethodInfo RegisterCoreMethod = typeof(DependencyInjection)
        .GetMethod(nameof(RegisterCore), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static IServiceCollection RegisterCore<TEvent, THandler>(IServiceCollection services)
        where TEvent : IIntegrationEvent
        where THandler : class, IEventConsumer<TEvent>
    {
        services.AddScoped<THandler>();
        var topic = THandler.Topic;
        services.AddHostedService(sp =>
        {
            var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
            var options = sp.GetRequiredService<IOptions<KafkaOptions>>();
            var logger = sp.GetRequiredService<ILogger<KafkaConsumerHost<TEvent, THandler>>>();
            return new KafkaConsumerHost<TEvent, THandler>(topic, scopeFactory, options, logger);
        });

        return services;
    }
}
