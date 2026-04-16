using System.Text.Json;
using Shared.Contracts.IntegrationEvents;
using Shared.Messaging.Abstractions;

namespace Order.Infrastructure.Outbox;

/// <summary>
///     Publishes events by writing to the outbox table (same DB transaction).
///     The OutboxProcessor background service picks them up and sends to Kafka.
/// </summary>
public class OutboxEventBus(IOutboxStore outboxStore) : IEventBus
{
    public Task PublishAsync<TEvent>(TEvent @event, string topic, string? partitionKey = null,
        CancellationToken ct = default) where TEvent : IIntegrationEvent
    {
        var message = new OutboxMessage
        {
            Topic = topic,
            PartitionKey = partitionKey ?? @event.EventId.ToString(),
            EventType = typeof(TEvent).AssemblyQualifiedName!,
            Payload = JsonSerializer.Serialize(@event)
        };

        outboxStore.Add(message);
        return Task.CompletedTask;
    }
}
