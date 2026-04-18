using Inventory.Domain;
using Inventory.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Shared.Contracts;
using Shared.Contracts.IntegrationEvents;
using Shared.Core.CQRS;
using Shared.Messaging.Abstractions;

namespace Inventory.Application.EventHandlers;

public class OrderPlacedConsumer(
    IInventoryRepository inventoryRepo,
    IUnitOfWork uow,
    IEventBus eventBus,
    ILogger<OrderPlacedConsumer> logger)
    : IEventConsumer<OrderPlacedIntegrationEvent>
{
    public async Task HandleAsync(OrderPlacedIntegrationEvent @event, CancellationToken ct = default)
    {
        logger.LogInformation("Reserving inventory for order {OrderId} ({OrderNumber})",
            @event.OrderId, @event.OrderNumber);

        var productIds = @event.Items.Select(i => i.ProductId).ToList();
        var items = await inventoryRepo.GetByProductIdsAsync(productIds, ct);
        var itemMap = items.ToDictionary(i => i.ProductId);

        // Phase 1 — validate every line before mutating any entity. Publishing a
        // failure here is safe because no Reserve() has been called yet, so
        // SaveChangesAsync only persists the outbox row.
        foreach (var line in @event.Items)
        {
            if (!itemMap.TryGetValue(line.ProductId, out var item))
            {
                await PublishFailure(@event.OrderId,
                    DomainErrors.InventoryItem.NotFound(line.ProductId).Message, ct);
                return;
            }

            if (item.Available < line.Quantity)
            {
                await PublishFailure(@event.OrderId,
                    DomainErrors.InventoryItem.InsufficientStock(item.ProductName, item.Available, line.Quantity).Message,
                    ct);
                return;
            }
        }

        // Phase 2 — pre-check passed, so every Reserve() must succeed.
        // If any fails, throw before SaveChangesAsync so partial reservations
        // are not persisted alongside the success event.
        var reservedItems = new List<ReservedItem>(@event.Items.Count);
        foreach (var line in @event.Items)
        {
            var item = itemMap[line.ProductId];
            var reserveResult = item.Reserve(line.Quantity);
            if (reserveResult.IsFailure)
            {
                throw new InvalidOperationException(
                    $"Reserve failed for product {line.ProductId} after pre-check passed: {reserveResult.Error.Message}");
            }

            reservedItems.Add(new ReservedItem(item.ProductId, line.Quantity));
        }

        await eventBus.PublishAsync(
            new StockReservedIntegrationEvent(
                Guid.NewGuid(),
                DateTime.UtcNow,
                @event.OrderId,
                reservedItems),
            Topics.StockReserved,
            @event.OrderId.ToString(),
            ct);

        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Inventory reserved for order {OrderId}", @event.OrderId);
    }

    private async Task PublishFailure(Guid orderId, string reason, CancellationToken ct)
    {
        logger.LogWarning("Inventory reservation failed for order {OrderId}: {Reason}", orderId, reason);

        await eventBus.PublishAsync(
            new StockReservationFailedIntegrationEvent(
                Guid.NewGuid(),
                DateTime.UtcNow,
                orderId,
                reason),
            Topics.StockReservationFailed,
            orderId.ToString(),
            ct);

        await uow.SaveChangesAsync(ct);
    }
}
