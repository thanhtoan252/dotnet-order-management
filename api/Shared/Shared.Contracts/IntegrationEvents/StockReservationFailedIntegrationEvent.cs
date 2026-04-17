namespace Shared.Contracts.IntegrationEvents;

public sealed record StockReservationFailedIntegrationEvent(
    Guid EventId,
    DateTime OccurredOn,
    Guid OrderId,
    string Reason) : IIntegrationEvent;
