namespace Shared.Contracts.IntegrationEvents;

public sealed record ProductCreatedIntegrationEvent(
    Guid EventId,
    DateTime OccurredOn,
    Guid ProductId,
    string Sku,
    string Name,
    int InitialStockQuantity) : IIntegrationEvent;
