using Catalog.Domain;
using Catalog.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Shared.Contracts;
using Shared.Contracts.IntegrationEvents;
using Shared.Core.CQRS;
using Shared.Core.Domain;
using Shared.Messaging.Abstractions;

namespace Catalog.Application.Products.Commands;

public record DeleteProductCommand(Guid ProductId)
    : ICommand<Result>;

public class DeleteProductHandler(
    IProductRepository productRepo,
    IUnitOfWork uow,
    IEventBus eventBus,
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

        await eventBus.PublishAsync(
            new ProductDeletedIntegrationEvent(
                Guid.NewGuid(),
                DateTime.UtcNow,
                product.Id),
            Topics.ProductDeleted,
            product.Id.ToString(),
            ct);

        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Product {Id} deleted.", command.ProductId);

        return Result.Success();
    }
}
