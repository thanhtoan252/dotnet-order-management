namespace Shared.Contracts.IntegrationEvents;

public sealed record ProductDeletedIntegrationEvent(
    Guid EventId,
    DateTime OccurredOn,
    Guid ProductId) : IIntegrationEvent;
