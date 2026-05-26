namespace Shared.Contracts;

public record StockCheckResponse(bool IsAvailable, IReadOnlyList<StockCheckFailure> Failures);
