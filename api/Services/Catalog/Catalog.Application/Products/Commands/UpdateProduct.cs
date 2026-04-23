using Catalog.Application.Abstractions;
using Catalog.Application.Products.Mappers;
using Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Contracts;
using Shared.Contracts.IntegrationEvents;
using Shared.Core.CQRS;
using Shared.Core.Domain;
using Shared.Core.ValueObjects;
using Shared.Messaging.Abstractions;

namespace Catalog.Application.Products.Commands;

public record UpdateProductCommand(Guid ProductId, UpdateProductRequest Request)
    : ICommand<Result<ProductResponse>>;

public class UpdateProductHandler(
    ICatalogDbContext db,
    IEventBus eventBus,
    ILogger<UpdateProductHandler> logger)
    : ICommandHandler<UpdateProductCommand, Result<ProductResponse>>
{
    public async Task<Result<ProductResponse>> HandleAsync(UpdateProductCommand command, CancellationToken ct)
    {
        var product = await db.Products.SingleOrDefaultAsync(p => p.Id == command.ProductId, ct);
        if (product is null)
        {
            return DomainErrors.Product.NotFound(command.ProductId);
        }

        var request = command.Request;
        var nameChanged = false;

        if (request.Name is not null)
        {
            var previousName = product.Name;
            var nameResult = product.UpdateName(request.Name);
            if (nameResult.IsFailure)
            {
                return nameResult.Error;
            }
            nameChanged = !string.Equals(previousName, product.Name, StringComparison.Ordinal);
        }

        if (request.Price.HasValue)
        {
            var priceResult = Money.Create(request.Price.Value, request.Currency ?? product.Price.Currency);
            if (priceResult.IsFailure)
            {
                return priceResult.Error;
            }

            product.UpdatePrice(priceResult.Value);
        }

        if (nameChanged)
        {
            await eventBus.PublishAsync(
                new ProductRenamedIntegrationEvent(
                    Guid.NewGuid(),
                    DateTime.UtcNow,
                    product.Id,
                    product.Name),
                Topics.ProductRenamed,
                product.Id.ToString(),
                ct);
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Product {Id} updated.", command.ProductId);

        return product.ToCommandResponse();
    }
}
