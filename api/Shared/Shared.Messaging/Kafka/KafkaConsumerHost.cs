using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Contracts.IntegrationEvents;
using Shared.Messaging.Abstractions;
using Shared.Messaging.Diagnostics;

namespace Shared.Messaging.Kafka;

/// <summary>
///     Background service that consumes messages from a Kafka topic and dispatches to the registered handler.
///     Supports idempotency via ProcessedMessage table and retry with dead-letter topic.
/// </summary>
public class KafkaConsumerHost<TEvent, THandler> : BackgroundService
    where TEvent : IIntegrationEvent
    where THandler : class, IEventConsumer<TEvent>
{
    private readonly string _topic;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<KafkaConsumerHost<TEvent, THandler>> _logger;
    private readonly KafkaOptions _options;

    public KafkaConsumerHost(
        string topic,
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaOptions> options,
        ILogger<KafkaConsumerHost<TEvent, THandler>> logger)
    {
        _topic = topic;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        using var consumer = CreateConsumer();
        consumer.Subscribe(_topic);

        _logger.LogInformation("Kafka consumer started for topic {Topic} with group {GroupId}", _topic,
            _options.GroupId);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConsumeNextAsync(consumer, stoppingToken);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Kafka consume error on topic {Topic}", _topic);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in consumer loop for {Topic}", _topic);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private IConsumer<string, string> CreateConsumer()
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        return new ConsumerBuilder<string, string>(config).Build();
    }

    private async Task ConsumeNextAsync(IConsumer<string, string> consumer, CancellationToken stoppingToken)
    {
        var result = consumer.Consume(stoppingToken);
        if (result?.Message?.Value is null)
        {
            return;
        }

        using var activity = StartReceiveActivity(result);

        var @event = JsonSerializer.Deserialize<TEvent>(result.Message.Value);
        if (@event is null)
        {
            _logger.LogWarning("Failed to deserialize message from {Topic}", _topic);
            consumer.Commit(result);
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var idempotencyStore = scope.ServiceProvider.GetRequiredService<IIdempotencyStore>();

        if (await idempotencyStore.HasBeenProcessedAsync(@event.EventId, stoppingToken))
        {
            _logger.LogDebug("Event {EventId} already processed, skipping", @event.EventId);
            consumer.Commit(result);
            return;
        }

        var handled = await TryHandleWithRetryAsync(@event, stoppingToken);

        if (handled)
        {
            await idempotencyStore.MarkEventProcessedAsync(@event.EventId, typeof(TEvent).Name, stoppingToken);
            _logger.LogInformation("Processed {EventType} {EventId} from {Topic}",
                typeof(TEvent).Name, @event.EventId, _topic);
        }
        else
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Max retries exceeded");
            await PublishToDeadLetterAsync(result.Message, stoppingToken);
            _logger.LogError(
                "Message {EventId} from {Topic} moved to dead-letter topic after {MaxRetries} failed attempts",
                @event.EventId, _topic, _options.MaxRetryAttempts);
        }

        consumer.Commit(result);
    }

    private Activity? StartReceiveActivity(ConsumeResult<string, string> result)
    {
        var parentContext = ExtractParentContext(result.Message.Headers);

        var activity = MessagingDiagnostics.ActivitySource.StartActivity(
            $"{_topic} receive",
            ActivityKind.Consumer,
            parentContext);

        activity?.SetTag("messaging.system", "kafka");
        activity?.SetTag("messaging.destination.name", _topic);
        activity?.SetTag("messaging.operation", "receive");
        activity?.SetTag("messaging.kafka.consumer.group", _options.GroupId);
        activity?.SetTag("messaging.kafka.destination.partition", result.Partition.Value);
        activity?.SetTag("messaging.kafka.message.offset", result.Offset.Value);

        return activity;
    }

    private Activity? StartDlqPublishActivity(string dlqTopic)
    {
        var activity = MessagingDiagnostics.ActivitySource.StartActivity(
            $"{dlqTopic} publish",
            ActivityKind.Producer);

        activity?.SetTag("messaging.system", "kafka");
        activity?.SetTag("messaging.destination.name", dlqTopic);
        activity?.SetTag("messaging.operation", "publish");
        activity?.SetTag("messaging.kafka.dlq.original_topic", _topic);

        return activity;
    }

    private static ActivityContext ExtractParentContext(Headers? headers)
    {
        if (headers is null)
        {
            return default;
        }

        string? traceparent = null;
        string? tracestate = null;
        foreach (var h in headers)
        {
            if (h.Key == "traceparent")
            {
                traceparent = Encoding.UTF8.GetString(h.GetValueBytes());
            }
            else if (h.Key == "tracestate")
            {
                tracestate = Encoding.UTF8.GetString(h.GetValueBytes());
            }
        }

        if (string.IsNullOrEmpty(traceparent))
        {
            return default;
        }

        return ActivityContext.TryParse(traceparent, tracestate, out var parsed) ? parsed : default;
    }

    private async Task<bool> TryHandleWithRetryAsync(TEvent @event, CancellationToken ct)
    {
        for (var attempt = 1; attempt <= _options.MaxRetryAttempts; attempt++)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<THandler>();
                await handler.HandleAsync(@event, ct);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Attempt {Attempt}/{MaxRetries} failed for {EventType} {EventId} on topic {Topic}",
                    attempt, _options.MaxRetryAttempts, typeof(TEvent).Name, @event.EventId, _topic);

                if (attempt < _options.MaxRetryAttempts)
                {
                    var delay = TimeSpan.FromSeconds(_options.RetryBaseDelaySeconds * Math.Pow(2, attempt - 1));
                    await Task.Delay(delay, ct);
                }
            }
        }

        return false;
    }

    private async Task PublishToDeadLetterAsync(Message<string, string> originalMessage, CancellationToken ct)
    {
        var dlqTopic = $"{_options.DeadLetterTopicPrefix}{_topic}";

        using var activity = StartDlqPublishActivity(dlqTopic);

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            Acks = Acks.All
        };

        using var producer = new ProducerBuilder<string, string>(producerConfig).Build();

        var dlqMessage = new Message<string, string>
        {
            Key = originalMessage.Key,
            Value = originalMessage.Value,
            Headers = originalMessage.Headers ?? new Headers()
        };

        dlqMessage.Headers.Add("x-original-topic", Encoding.UTF8.GetBytes(_topic));
        dlqMessage.Headers.Add("x-failure-timestamp", Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("O")));

        // Refresh trace-context headers on the DLQ message so consumers of the DLQ stay linked.
        if (activity is not null)
        {
            ReplaceHeader(dlqMessage.Headers, "traceparent", activity.Id ?? string.Empty);
            if (!string.IsNullOrEmpty(activity.TraceStateString))
            {
                ReplaceHeader(dlqMessage.Headers, "tracestate", activity.TraceStateString);
            }
        }

        try
        {
            var result = await producer.ProduceAsync(dlqTopic, dlqMessage, ct);
            activity?.SetTag("messaging.kafka.destination.partition", result.Partition.Value);
            activity?.SetTag("messaging.kafka.message.offset", result.Offset.Value);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }

        _logger.LogInformation("Published failed message to dead-letter topic {DlqTopic}", dlqTopic);
    }

    private static void ReplaceHeader(Headers headers, string key, string value)
    {
        headers.Remove(key);
        headers.Add(key, Encoding.UTF8.GetBytes(value));
    }
}
