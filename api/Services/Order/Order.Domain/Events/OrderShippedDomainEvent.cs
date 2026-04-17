using Shared.Core.Domain;

namespace Order.Domain.Events;

public sealed record OrderShippedDomainEvent(Guid OrderId, string OrderNumber) : IDomainEvent;
