using Microsoft.Extensions.Logging;
using OrderManagement.Application.Common.Interfaces;
using OrderManagement.Domain.Common;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Application.Products.Commands;

public record DeleteProductCommand(Guid ProductId)
    : ICommand<Result>;

public class DeleteProductHandler(
    IProductRepository productRepo,
    IUnitOfWork uow,
    ILogger<DeleteProductHandler> logger)
    : ICommandHandler<DeleteProductCommand, Result>
{
    public async Task<Result> HandleAsync(DeleteProductCommand command, CancellationToken ct)
    {
        var product = await productRepo.GetByIdAsync(command.ProductId, ct);
        if (product is null)
        {
            return DomainErrors.Product.NotFound(command.ProductId);
        }

        product.IsDeleted = true;
        productRepo.Update(product);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Product {Id} deleted.", command.ProductId);

        return Result.Success();
    }
}
