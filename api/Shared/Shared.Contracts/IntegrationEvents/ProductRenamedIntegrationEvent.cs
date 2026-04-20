namespace Shared.Contracts.IntegrationEvents;

public sealed record ProductRenamedIntegrationEvent(
    Guid EventId,
    DateTime OccurredOn,
    Guid ProductId,
    string NewName) : IIntegrationEvent;
