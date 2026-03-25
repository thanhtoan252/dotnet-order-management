using Microsoft.Extensions.Logging;
using OrderManagement.Application.Common.Interfaces;
using OrderManagement.Application.Products.Mappers;
using OrderManagement.Domain.Common;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Repositories;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Application.Products.Commands;

public record CreateProductCommand(CreateProductRequest Request)
    : ICommand<Result<ProductResponse>>;

public class CreateProductHandler(
    IProductRepository productRepo,
    IUnitOfWork uow,
    ILogger<CreateProductHandler> logger)
    : ICommandHandler<CreateProductCommand, Result<ProductResponse>>
{
    public async Task<Result<ProductResponse>> HandleAsync(CreateProductCommand command, CancellationToken ct)
    {
        var request = command.Request;
        var price = Money.Create(request.Price, request.Currency);
        var product = Product.Create(request.Name, request.Sku, price, request.StockQuantity, request.Description);

        productRepo.Add(product);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Product {Sku} created with Id {Id}.", product.SKU, product.Id);

        return product.ToCommandResponse();
    }
}
