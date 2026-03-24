using OrderManagement.Domain.Common;

namespace OrderManagement.Domain.Events;

public sealed record OrderPlacedDomainEvent(Guid OrderId, string OrderNumber, Guid CustomerId) : IDomainEvent;