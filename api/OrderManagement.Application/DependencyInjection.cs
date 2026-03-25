using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Application.Common.Dispatching;
using OrderManagement.Application.Common.Interfaces;
using OrderManagement.Domain.Common;
using OrderCmd = OrderManagement.Application.Orders.Commands;
using OrderQry = OrderManagement.Application.Orders.Queries;
using ProductCmd = OrderManagement.Application.Products.Commands;
using ProductQry = OrderManagement.Application.Products.Queries;

namespace OrderManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Dispatcher
        services.AddScoped<IDispatcher, Dispatcher>();

        // Order command handlers
        services.AddScoped<ICommandHandler<OrderCmd.PlaceOrderCommand, Result<OrderCmd.OrderResponse>>, OrderCmd.PlaceOrderHandler>();
        services.AddScoped<ICommandHandler<OrderCmd.ConfirmOrderCommand, Result<OrderCmd.OrderResponse>>, OrderCmd.ConfirmOrderHandler>();
        services.AddScoped<ICommandHandler<OrderCmd.ShipOrderCommand, Result<OrderCmd.OrderResponse>>, OrderCmd.ShipOrderHandler>();
        services.AddScoped<ICommandHandler<OrderCmd.CancelOrderCommand, Result<OrderCmd.OrderResponse>>, OrderCmd.CancelOrderHandler>();
        services.AddScoped<ICommandHandler<OrderCmd.DeliverOrderCommand, Result<OrderCmd.OrderResponse>>, OrderCmd.DeliverOrderHandler>();
        services.AddScoped<ICommandHandler<OrderCmd.DeleteOrderCommand, Result>, OrderCmd.DeleteOrderHandler>();

        // Order query handlers
        services.AddScoped<IQueryHandler<OrderQry.GetOrderByIdQuery, Result<OrderQry.OrderResponse>>, OrderQry.GetOrderByIdHandler>();
        services.AddScoped<IQueryHandler<OrderQry.GetAllOrdersQuery, IReadOnlyList<OrderQry.OrderResponse>>, OrderQry.GetAllOrdersHandler>();
        services.AddScoped<IQueryHandler<OrderQry.GetCustomerOrdersQuery, IReadOnlyList<OrderQry.OrderResponse>>, OrderQry.GetCustomerOrdersHandler>();

        // Product command handlers
        services.AddScoped<ICommandHandler<ProductCmd.CreateProductCommand, Result<ProductCmd.ProductResponse>>, ProductCmd.CreateProductHandler>();
        services.AddScoped<ICommandHandler<ProductCmd.UpdateProductCommand, Result<ProductCmd.ProductResponse>>, ProductCmd.UpdateProductHandler>();
        services.AddScoped<ICommandHandler<ProductCmd.DeleteProductCommand, Result>, ProductCmd.DeleteProductHandler>();

        // Product query handlers
        services.AddScoped<IQueryHandler<ProductQry.GetAllProductsQuery, IReadOnlyList<ProductQry.ProductResponse>>, ProductQry.GetAllProductsHandler>();

        return services;
    }
}
