using Inventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Messaging.Abstractions;

namespace Inventory.Infrastructure.Outbox;

public class OutboxStore(InventoryDbContext db) : IOutboxStore, IIdempotencyStore
{
    public void Add(OutboxMessage message)
    {
        db.OutboxMessages.Add(message);
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken ct = default)
    {
        return await db.OutboxMessages
            .Where(m => m.ProcessedAt == null && m.RetryCount < 3)
            .OrderBy(m => m.CreatedAt)
            .Take(batchSize)
            .ToListAsync(ct);
    }

    public async Task MarkProcessedAsync(Guid messageId, CancellationToken ct = default)
    {
        var message = await db.OutboxMessages.FindAsync([messageId], ct);
        if (message is not null)
        {
            message.ProcessedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task MarkFailedAsync(Guid messageId, string error, CancellationToken ct = default)
    {
        var message = await db.OutboxMessages.FindAsync([messageId], ct);
        if (message is not null)
        {
            message.RetryCount++;
            message.Error = error;
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> HasBeenProcessedAsync(Guid eventId, CancellationToken ct = default)
    {
        return await db.ProcessedMessages.AnyAsync(m => m.EventId == eventId, ct);
    }

    public async Task MarkEventProcessedAsync(Guid eventId, string eventType, CancellationToken ct = default)
    {
        db.ProcessedMessages.Add(new ProcessedMessage
        {
            EventId = eventId,
            EventType = eventType,
            ProcessedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);
    }
}
