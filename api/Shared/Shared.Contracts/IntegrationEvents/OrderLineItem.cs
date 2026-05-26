namespace Shared.Contracts.IntegrationEvents;

public sealed record OrderLineItem(Guid ProductId, int Quantity);
