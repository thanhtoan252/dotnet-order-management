using System.Diagnostics;
using System.Text;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Messaging.Diagnostics;

namespace Shared.Messaging.Kafka;

public sealed class KafkaProducer : IDisposable, IAsyncDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaProducer> _logger;

    public KafkaProducer(IOptions<KafkaOptions> options, ILogger<KafkaProducer> logger)
    {
        _logger = logger;
        var config = new ProducerConfig
        {
            BootstrapServers = options.Value.BootstrapServers,
            Acks = Acks.All,
            EnableIdempotence = true,
            MessageSendMaxRetries = 3,
            RetryBackoffMs = 1000
        };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task ProduceAsync(string topic, string key, string payload, CancellationToken ct = default)
    {
        using var activity = MessagingDiagnostics.ActivitySource.StartActivity(
            $"{topic} publish",
            ActivityKind.Producer);

        activity?.SetTag("messaging.system", "kafka");
        activity?.SetTag("messaging.destination.name", topic);
        activity?.SetTag("messaging.operation", "publish");
        activity?.SetTag("messaging.kafka.message.key", key);

        var headers = new Headers();
        if (activity is not null)
        {
            headers.Add("traceparent", Encoding.UTF8.GetBytes(activity.Id ?? string.Empty));
            if (!string.IsNullOrEmpty(activity.TraceStateString))
            {
                headers.Add("tracestate", Encoding.UTF8.GetBytes(activity.TraceStateString));
            }
        }

        var message = new Message<string, string>
        {
            Key = key,
            Value = payload,
            Headers = headers
        };

        try
        {
            var result = await _producer.ProduceAsync(topic, message, ct);
            activity?.SetTag("messaging.kafka.destination.partition", result.Partition.Value);
            activity?.SetTag("messaging.kafka.message.offset", result.Offset.Value);
            _logger.LogDebug("Produced message to {Topic} [{Partition}] @ offset {Offset}",
                result.Topic, result.Partition.Value, result.Offset.Value);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(10));
        _producer.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        Dispose();

        return ValueTask.CompletedTask;
    }
}
