namespace Inventory.Infrastructure.Outbox;

public interface IOutboxStore
{
    void Add(OutboxMessage message);
    Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken ct = default);
    Task MarkProcessedAsync(Guid messageId, CancellationToken ct = default);
    Task MarkFailedAsync(Guid messageId, string error, CancellationToken ct = default);
    Task<bool> HasBeenProcessedAsync(Guid eventId, CancellationToken ct = default);
    Task MarkEventProcessedAsync(Guid eventId, string eventType, CancellationToken ct = default);
}
