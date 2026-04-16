namespace Shared.Messaging.Kafka;

public class KafkaOptions
{
    public const string SectionName = "Kafka";

    public string BootstrapServers { get; set; } = "localhost:9092";
    public string GroupId { get; set; } = null!;

    /// <summary>Max retry attempts before sending to dead-letter topic. Default: 3.</summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>Initial delay between retries (exponential backoff). Default: 1 second.</summary>
    public int RetryBaseDelaySeconds { get; set; } = 1;

    /// <summary>Prefix for dead-letter topics. E.g. topic "order.placed" → "dlq.order.placed".</summary>
    public string DeadLetterTopicPrefix { get; set; } = "dlq.";
}
