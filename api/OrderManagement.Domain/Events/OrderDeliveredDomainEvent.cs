using OrderManagement.Domain.Common;

namespace OrderManagement.Domain.Events;

public sealed record OrderDeliveredDomainEvent(Guid OrderId, string OrderNumber) : IDomainEvent;