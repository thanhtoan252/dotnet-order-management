using Microsoft.Extensions.Logging;
using Order.Application.Abstractions;
using Order.Application.Orders.Mappers;
using Order.Application.Services;
using Order.Domain;
using Order.Domain.Entities;
using Shared.Contracts;
using Shared.Contracts.IntegrationEvents;
using Shared.Core.CQRS;
using Shared.Core.Domain;
using Shared.Core.ValueObjects;
using Shared.Messaging.Abstractions;

namespace Order.Application.Orders.Commands;

public record PlaceOrderCommand(PlaceOrderRequest Request, string PlacedBy)
    : ICommand<Result<OrderResponse>>;

public class PlaceOrderHandler(
    IOrderDbContext db,
    IEventBus eventBus,
    IInventoryService inventoryService,
    ILogger<PlaceOrderHandler> logger)
    : ICommandHandler<PlaceOrderCommand, Result<OrderResponse>>
{
    public async Task<Result<OrderResponse>> HandleAsync(PlaceOrderCommand command, CancellationToken ct)
    {
        var request = command.Request;

        // Synchronous availability check against Inventory before creating the order
        var stockItems = request.Lines
            .Select(l => new StockCheckItem(l.ProductId, l.Quantity))
            .ToList();

        var stockCheck = await inventoryService.CheckAvailabilityAsync(new StockCheckRequest(stockItems), ct);
        if (!stockCheck.IsAvailable)
        {
            var reasons = string.Join("; ", stockCheck.Failures.Select(f => f.Reason));
            logger.LogWarning("Stock check failed for order request: {Reasons}", reasons);

            return DomainErrors.Order.InsufficientStock(reasons);
        }

        var address = Address.Create(
            request.ShippingAddress.Street,
            request.ShippingAddress.City,
            request.ShippingAddress.Province,
            request.ShippingAddress.ZipCode);

        var createResult = OrderAggregate.Create(request.CustomerId, address, request.Notes);
        if (createResult.IsFailure)
        {
            return createResult.Error;
        }

        var order = createResult.Value;
        order.CreatedBy = command.PlacedBy;

        foreach (var line in request.Lines)
        {
            var priceResult = Money.Create(line.UnitPrice, line.Currency ?? "USD");
            if (priceResult.IsFailure)
            {
                return priceResult.Error;
            }

            var addResult = order.AddItem(
                line.ProductId,
                line.ProductName ?? $"Product-{line.ProductId}",
                priceResult.Value,
                line.Quantity);

            if (addResult.IsFailure)
            {
                return addResult.Error;
            }
        }

        db.Orders.Add(order);

        // Publish integration event for Catalog Service to reserve stock
        var integrationEvent = new OrderPlacedIntegrationEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            order.Id,
            order.OrderNumber,
            order.CustomerId,
            order.Items.Select(i => new OrderLineItem(i.ProductId, i.Quantity)).ToList());

        await eventBus.PublishAsync(integrationEvent, Topics.OrderPlaced, order.Id.ToString(), ct);

        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "Order {OrderNumber} placed by {User} for customer {CustomerId} with {ItemCount} item(s). Total: {Total}. Awaiting stock reservation.",
            order.OrderNumber, command.PlacedBy, request.CustomerId, order.Items.Count, order.TotalAmount);

        return order.ToCommandResponse();
    }
}
