namespace Shared.Contracts;

public record StockCheckRequest(IReadOnlyList<StockCheckItem> Items);
