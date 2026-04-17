using Microsoft.Extensions.DependencyInjection;
using Shared.Core.CQRS;
using Shared.Core.Domain;
using OrderCmd = Order.Application.Orders.Commands;
using OrderQry = Order.Application.Orders.Queries;

namespace Order.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
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

        return services;
    }
}
