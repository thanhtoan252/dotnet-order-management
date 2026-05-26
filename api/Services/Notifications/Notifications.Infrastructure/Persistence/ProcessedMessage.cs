namespace Notifications.Infrastructure.Persistence;

public sealed class ProcessedMessage
{
    public Guid EventId { get; set; }
    public string EventType { get; set; } = null!;
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
