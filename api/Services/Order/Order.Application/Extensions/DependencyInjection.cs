using Microsoft.Extensions.DependencyInjection;
using Shared.Core.CQRS;
using Shared.Core.Domain;
using Order.Application.Orders.Models;
using OrderCmd = Order.Application.Orders.Commands;
using OrderQry = Order.Application.Orders.Queries;

namespace Order.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDispatcher, Dispatcher>();

        // Order command handlers
        services.AddScoped<ICommandHandler<OrderCmd.PlaceOrderCommand, Result<OrderResult>>,
            OrderCmd.PlaceOrderHandler>();
        services.AddScoped<ICommandHandler<OrderCmd.ConfirmOrderCommand, Result<OrderResult>>,
            OrderCmd.ConfirmOrderHandler>();
        services.AddScoped<ICommandHandler<OrderCmd.ShipOrderCommand, Result<OrderResult>>,
            OrderCmd.ShipOrderHandler>();
        services.AddScoped<ICommandHandler<OrderCmd.CancelOrderCommand, Result<OrderResult>>,
            OrderCmd.CancelOrderHandler>();
        services.AddScoped<ICommandHandler<OrderCmd.DeliverOrderCommand, Result<OrderResult>>,
            OrderCmd.DeliverOrderHandler>();
        services.AddScoped<ICommandHandler<OrderCmd.DeleteOrderCommand, Result>, OrderCmd.DeleteOrderHandler>();

        // Order query handlers
        services.AddScoped<IQueryHandler<OrderQry.GetOrderByIdQuery, Result<OrderResult>>,
            OrderQry.GetOrderByIdHandler>();
        services.AddScoped<IQueryHandler<OrderQry.GetAllOrdersQuery, IReadOnlyList<OrderResult>>,
            OrderQry.GetAllOrdersHandler>();
        services.AddScoped<
            IQueryHandler<OrderQry.GetCustomerOrdersQuery, IReadOnlyList<OrderResult>>,
            OrderQry.GetCustomerOrdersHandler>();

        return services;
    }
}
