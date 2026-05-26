namespace Shared.Contracts;

public record StockCheckFailure(Guid ProductId, string Reason);
