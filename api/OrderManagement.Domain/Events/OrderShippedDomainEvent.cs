using OrderManagement.Domain.Common;

namespace OrderManagement.Domain.Events;

public sealed record OrderShippedDomainEvent(Guid OrderId, string OrderNumber) : IDomainEvent;