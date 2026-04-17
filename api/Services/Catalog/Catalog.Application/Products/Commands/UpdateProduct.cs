using Catalog.Application.Products.Mappers;
using Catalog.Domain;
using Catalog.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Shared.Core.CQRS;
using Shared.Core.Domain;
using Shared.Core.ValueObjects;

namespace Catalog.Application.Products.Commands;

public record UpdateProductCommand(Guid ProductId, UpdateProductRequest Request)
    : ICommand<Result<ProductResponse>>;

public class UpdateProductHandler(IProductRepository productRepo, IUnitOfWork uow, ILogger<UpdateProductHandler> logger)
    : ICommandHandler<UpdateProductCommand, Result<ProductResponse>>
{
    public async Task<Result<ProductResponse>> HandleAsync(UpdateProductCommand command, CancellationToken ct)
    {
        var product = await productRepo.GetByIdAsync(command.ProductId, ct);
        if (product is null)
        {
            return DomainErrors.Product.NotFound(command.ProductId);
        }

        var request = command.Request;

        if (request.Name is not null)
        {
            var nameResult = product.UpdateName(request.Name);
            if (nameResult.IsFailure)
            {
                return nameResult.Error;
            }
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

        if (request.StockQuantity.HasValue)
        {
            var diff = request.StockQuantity.Value - product.StockQuantity;
            if (diff > 0)
            {
                var restockResult = product.AddStock(diff);
                if (restockResult.IsFailure)
                {
                    return restockResult.Error;
                }
            }
            else if (diff < 0)
            {
                var deductResult = product.DeductStock(-diff);
                if (deductResult.IsFailure)
                {
                    return deductResult.Error;
                }
            }
        }

        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Product {Id} updated.", command.ProductId);

        return product.ToCommandResponse();
    }
}
