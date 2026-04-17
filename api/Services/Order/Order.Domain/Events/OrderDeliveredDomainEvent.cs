using Shared.Core.Domain;

namespace Order.Domain.Events;

public sealed record OrderDeliveredDomainEvent(Guid OrderId, string OrderNumber) : IDomainEvent;
