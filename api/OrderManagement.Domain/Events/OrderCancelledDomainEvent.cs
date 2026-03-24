using OrderManagement.Domain.Common;

namespace OrderManagement.Domain.Events;

public sealed record OrderCancelledDomainEvent(Guid OrderId, string OrderNumber, string Reason) : IDomainEvent;