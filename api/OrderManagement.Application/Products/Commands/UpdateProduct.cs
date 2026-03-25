using Microsoft.Extensions.Logging;
using OrderManagement.Application.Common.Interfaces;
using OrderManagement.Application.Products.Mappers;
using OrderManagement.Domain.Common;
using OrderManagement.Domain.Repositories;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Application.Products.Commands;

public record UpdateProductCommand(Guid ProductId, UpdateProductRequest Request)
    : ICommand<Result<ProductResponse>>;

public class UpdateProductHandler(
    IProductRepository productRepo,
    IUnitOfWork uow,
    ILogger<UpdateProductHandler> logger)
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
            var price = Money.Create(request.Price.Value, request.Currency);
            product.UpdatePrice(price);
        }

        if (request.StockQuantity.HasValue)
        {
            var diff = request.StockQuantity.Value - product.StockQuantity;
            if (diff > 0)
            {
                var restockResult = product.Restock(diff);
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

        productRepo.Update(product);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Product {Id} updated.", command.ProductId);

        return product.ToCommandResponse();
    }
}
