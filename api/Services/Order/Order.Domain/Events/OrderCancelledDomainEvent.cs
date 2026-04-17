using Shared.Core.Domain;

namespace Order.Domain.Events;

public sealed record OrderCancelledDomainEvent(Guid OrderId, string OrderNumber, string Reason) : IDomainEvent;
