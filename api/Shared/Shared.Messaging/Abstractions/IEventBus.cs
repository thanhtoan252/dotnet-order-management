using Shared.Contracts.IntegrationEvents;

namespace Shared.Messaging.Abstractions;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, string topic, string? partitionKey = null, CancellationToken ct = default)
        where TEvent : IIntegrationEvent;
}
