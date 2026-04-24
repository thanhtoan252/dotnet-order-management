using Shared.Contracts.IntegrationEvents;

namespace Shared.Messaging.Abstractions;

public interface IEventConsumer<in TEvent> where TEvent : IIntegrationEvent
{
    Task HandleAsync(TEvent @event, CancellationToken ct = default);
}
