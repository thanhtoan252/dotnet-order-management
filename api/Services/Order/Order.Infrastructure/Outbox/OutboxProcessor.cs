using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Messaging.Kafka;

namespace Order.Infrastructure.Outbox;

/// <summary>
///     Background service that polls the outbox table and publishes pending messages to Kafka.
/// </summary>
public class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    KafkaProducer kafkaProducer,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    private const int BatchSize = 50;
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox processor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var outboxStore = scope.ServiceProvider.GetRequiredService<IOutboxStore>();

                var messages = await outboxStore.GetPendingAsync(BatchSize, stoppingToken);

                foreach (var message in messages)
                {
                    try
                    {
                        await kafkaProducer.ProduceAsync(
                            message.Topic,
                            message.PartitionKey,
                            message.Payload,
                            stoppingToken);

                        await outboxStore.MarkProcessedAsync(message.Id, stoppingToken);

                        logger.LogDebug("Outbox message {MessageId} published to {Topic}", message.Id, message.Topic);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to publish outbox message {MessageId} to {Topic}",
                            message.Id, message.Topic);
                        await outboxStore.MarkFailedAsync(message.Id, ex.Message, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox processor error");
            }

            await Task.Delay(PollingInterval, stoppingToken);
        }
    }
}
