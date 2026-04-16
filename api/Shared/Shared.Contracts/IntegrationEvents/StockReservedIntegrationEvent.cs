namespace Shared.Contracts.IntegrationEvents;

public sealed record StockReservedIntegrationEvent(
    Guid EventId,
    DateTime OccurredOn,
    Guid OrderId,
    IReadOnlyList<ReservedItem> Items) : IIntegrationEvent;

public sealed record ReservedItem(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string Currency,
    int Quantity);
