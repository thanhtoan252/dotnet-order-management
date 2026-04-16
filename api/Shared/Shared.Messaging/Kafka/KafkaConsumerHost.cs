using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Contracts.IntegrationEvents;
using Shared.Messaging.Abstractions;

namespace Shared.Messaging.Kafka;

/// <summary>
///     Background service that consumes messages from a Kafka topic and dispatches to the registered handler.
///     Supports idempotency via ProcessedMessage table and retry with dead-letter topic.
/// </summary>
public class KafkaConsumerHost<TEvent> : BackgroundService where TEvent : IIntegrationEvent
{
    private readonly string _topic;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<KafkaConsumerHost<TEvent>> _logger;
    private readonly KafkaOptions _options;

    public KafkaConsumerHost(
        string topic,
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaOptions> options,
        ILogger<KafkaConsumerHost<TEvent>> logger)
    {
        _topic = topic;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        var config = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(_topic);

        _logger.LogInformation("Kafka consumer started for topic {Topic} with group {GroupId}", _topic,
            _options.GroupId);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                if (result?.Message?.Value is null)
                {
                    continue;
                }

                var @event = JsonSerializer.Deserialize<TEvent>(result.Message.Value);
                if (@event is null)
                {
                    _logger.LogWarning("Failed to deserialize message from {Topic}", _topic);
                    consumer.Commit(result);
                    continue;
                }

                using var scope = _scopeFactory.CreateScope();

                // Idempotency check
                var outboxStore = scope.ServiceProvider.GetRequiredService<IIdempotencyStore>();
                if (await outboxStore.HasBeenProcessedAsync(@event.EventId, stoppingToken))
                {
                    _logger.LogDebug("Event {EventId} already processed, skipping", @event.EventId);
                    consumer.Commit(result);
                    continue;
                }

                var handled = await TryHandleWithRetryAsync(@event, stoppingToken);

                if (handled)
                {
                    await outboxStore.MarkEventProcessedAsync(@event.EventId, typeof(TEvent).Name, stoppingToken);
                    _logger.LogInformation("Processed {EventType} {EventId} from {Topic}",
                        typeof(TEvent).Name, @event.EventId, _topic);
                }
                else
                {
                    await PublishToDeadLetterAsync(result.Message, stoppingToken);
                    _logger.LogError(
                        "Message {EventId} from {Topic} moved to dead-letter topic after {MaxRetries} failed attempts",
                        @event.EventId, _topic, _options.MaxRetryAttempts);
                }

                consumer.Commit(result);
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

    private async Task<bool> TryHandleWithRetryAsync(TEvent @event, CancellationToken ct)
    {
        for (var attempt = 1; attempt <= _options.MaxRetryAttempts; attempt++)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<Abstractions.IEventConsumer<TEvent>>();
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

        dlqMessage.Headers.Add("x-original-topic", System.Text.Encoding.UTF8.GetBytes(_topic));
        dlqMessage.Headers.Add("x-failure-timestamp", System.Text.Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("O")));

        await producer.ProduceAsync(dlqTopic, dlqMessage, ct);

        _logger.LogInformation("Published failed message to dead-letter topic {DlqTopic}", dlqTopic);
    }
}
