namespace Inventory.Infrastructure.Outbox;

public class ProcessedMessage
{
    public Guid EventId { get; set; }
    public string EventType { get; set; } = null!;
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
