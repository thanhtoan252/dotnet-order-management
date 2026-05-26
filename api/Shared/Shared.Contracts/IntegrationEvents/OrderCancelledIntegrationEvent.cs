namespace Shared.Contracts.IntegrationEvents;

public sealed record OrderCancelledIntegrationEvent(
    Guid EventId,
    DateTime OccurredOn,
    Guid OrderId,
    string OrderNumber,
    Guid UserId,
    string Reason,
    IReadOnlyList<OrderLineItem> Items) : IIntegrationEvent;
