using Shared.Core.Domain;

namespace Order.Domain.Events;

public sealed record OrderConfirmedDomainEvent(Guid OrderId, string OrderNumber) : IDomainEvent;
