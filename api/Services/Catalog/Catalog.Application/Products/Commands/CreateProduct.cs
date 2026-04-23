using Catalog.Application.Products.Mappers;
using Catalog.Domain.Entities;
using Catalog.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Shared.Contracts;
using Shared.Contracts.IntegrationEvents;
using Shared.Core.CQRS;
using Shared.Core.Domain;
using Shared.Core.ValueObjects;
using Shared.Messaging.Abstractions;

namespace Catalog.Application.Products.Commands;

public record CreateProductCommand(CreateProductRequest Request)
    : ICommand<Result<ProductResponse>>;

public class CreateProductHandler(
    IProductRepository productRepo,
    IUnitOfWork uow,
    IEventBus eventBus,
    ILogger<CreateProductHandler> logger)
    : ICommandHandler<CreateProductCommand, Result<ProductResponse>>
{
    public async Task<Result<ProductResponse>> HandleAsync(CreateProductCommand command, CancellationToken ct)
    {
        var request = command.Request;
        var priceResult = Money.Create(request.Price, request.Currency);
        if (priceResult.IsFailure)
        {
            return priceResult.Error;
        }

        var productResult = Product.Create(request.Name, request.Sku, priceResult.Value, request.Description);

        if (productResult.IsFailure)
        {
            return productResult.Error;
        }

        var product = productResult.Value;
        productRepo.Add(product);

        await eventBus.PublishAsync(
            new ProductCreatedIntegrationEvent(
                Guid.NewGuid(),
                DateTime.UtcNow,
                product.Id,
                product.SKU,
                product.Name,
                request.InitialStockQuantity ?? 0),
            Topics.ProductCreated,
            product.Id.ToString(),
            ct);

        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Product {Sku} created with Id {Id}.", product.SKU, product.Id);

        return product.ToCommandResponse();
    }
}
