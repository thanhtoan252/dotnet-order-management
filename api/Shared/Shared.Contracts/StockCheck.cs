namespace Shared.Contracts;

public record StockCheckRequest(IReadOnlyList<StockCheckItem> Items);

public record StockCheckItem(Guid ProductId, int Quantity);

public record StockCheckResponse(bool IsAvailable, IReadOnlyList<StockCheckFailure> Failures);

public record StockCheckFailure(Guid ProductId, string Reason);
