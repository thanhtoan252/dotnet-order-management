using Microsoft.EntityFrameworkCore;
using Notifications.Infrastructure.Data;
using Shared.Messaging.Abstractions;

namespace Notifications.Infrastructure.Persistence;

public sealed class ProcessedMessageStore(NotificationsDbContext db) : IIdempotencyStore
{
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
