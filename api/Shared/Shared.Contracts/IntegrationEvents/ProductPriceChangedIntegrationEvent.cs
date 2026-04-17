namespace Shared.Contracts.IntegrationEvents;

public sealed record ProductPriceChangedIntegrationEvent(
    Guid EventId,
    DateTime OccurredOn,
    Guid ProductId,
    decimal NewPrice,
    string Currency) : IIntegrationEvent;
