using Microsoft.Extensions.DependencyInjection;
using Shared.Core.CQRS;
using Shared.Core.Domain;
using ProductCmd = Catalog.Application.Products.Commands;
using ProductQry = Catalog.Application.Products.Queries;

namespace Catalog.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDispatcher, Dispatcher>();

        // Product command handlers
        services.AddScoped<ICommandHandler<ProductCmd.CreateProductCommand, Result<ProductCmd.ProductResponse>>, ProductCmd.CreateProductHandler>();
        services.AddScoped<ICommandHandler<ProductCmd.UpdateProductCommand, Result<ProductCmd.ProductResponse>>, ProductCmd.UpdateProductHandler>();
        services.AddScoped<ICommandHandler<ProductCmd.DeleteProductCommand, Result>, ProductCmd.DeleteProductHandler>();

        // Product query handlers
        services.AddScoped<IQueryHandler<ProductQry.GetAllProductsQuery, IReadOnlyList<ProductQry.ProductResponse>>, ProductQry.GetAllProductsHandler>();
        services.AddScoped<IQueryHandler<ProductQry.CheckStockQuery, Shared.Contracts.StockCheckResponse>, ProductQry.CheckStockHandler>();

        return services;
    }
}
