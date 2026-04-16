using Shared.Core.Domain;

namespace Order.Domain.Events;

public sealed record OrderPlacedDomainEvent(Guid OrderId, string OrderNumber, Guid CustomerId) : IDomainEvent;
