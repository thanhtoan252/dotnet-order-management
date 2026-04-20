namespace Shared.Contracts.IntegrationEvents;

public sealed record OrderPlacedIntegrationEvent(
    Guid EventId,
    DateTime OccurredOn,
    Guid OrderId,
    string OrderNumber,
    Guid CustomerId,
    IReadOnlyList<OrderLineItem> Items) : IIntegrationEvent;

public sealed record OrderLineItem(Guid ProductId, int Quantity);
