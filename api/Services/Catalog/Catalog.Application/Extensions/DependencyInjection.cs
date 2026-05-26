using Microsoft.Extensions.DependencyInjection;
using Shared.Core.CQRS;
using Shared.Core.Domain;
using Catalog.Application.Products.Models;
using ProductCmd = Catalog.Application.Products.Commands;
using ProductQry = Catalog.Application.Products.Queries;

namespace Catalog.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDispatcher, Dispatcher>();

        // Product command handlers
        services.AddScoped<
            ICommandHandler<ProductCmd.CreateProductCommand, Result<ProductResult>>,
            ProductCmd.CreateProductHandler>();
        services.AddScoped<
            ICommandHandler<ProductCmd.UpdateProductCommand, Result<ProductResult>>,
            ProductCmd.UpdateProductHandler>();
        services.AddScoped<ICommandHandler<ProductCmd.DeleteProductCommand, Result>, ProductCmd.DeleteProductHandler>();
        services.AddScoped<
            ICommandHandler<ProductCmd.ImportProductsCommand, Result<ImportProductsResult>>,
            ProductCmd.ImportProductsHandler>();

        // Product query handlers
        services.AddScoped<
            IQueryHandler<ProductQry.GetAllProductsQuery, IReadOnlyList<ProductResult>>,
            ProductQry.GetAllProductsHandler>();

        return services;
    }
}
