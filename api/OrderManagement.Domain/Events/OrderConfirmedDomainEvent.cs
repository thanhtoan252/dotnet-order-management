using OrderManagement.Domain.Common;

namespace OrderManagement.Domain.Events;

public sealed record OrderConfirmedDomainEvent(Guid OrderId, string OrderNumber) : IDomainEvent;