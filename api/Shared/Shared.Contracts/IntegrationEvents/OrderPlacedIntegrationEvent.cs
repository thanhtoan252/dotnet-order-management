namespace Shared.Contracts.IntegrationEvents;

public sealed record OrderPlacedIntegrationEvent(
    Guid EventId,
    DateTime OccurredOn,
    Guid OrderId,
    string OrderNumber,
    Guid CustomerId,
    Guid UserId,
    IReadOnlyList<OrderLineItem> Items) : IIntegrationEvent;
