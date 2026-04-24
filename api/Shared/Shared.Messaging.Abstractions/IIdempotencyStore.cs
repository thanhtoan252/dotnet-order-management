namespace Shared.Messaging.Abstractions;

public interface IIdempotencyStore
{
    Task<bool> HasBeenProcessedAsync(Guid eventId, CancellationToken ct = default);
    Task MarkEventProcessedAsync(Guid eventId, string eventType, CancellationToken ct = default);
}
