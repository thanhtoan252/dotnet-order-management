using Shared.Contracts.IntegrationEvents;

namespace Shared.Messaging.Abstractions;

public interface IEventConsumer<in TEvent> where TEvent : IIntegrationEvent
{
    static abstract string Topic { get; }

    Task HandleAsync(TEvent @event, CancellationToken ct = default);
}
