using Microsoft.Extensions.DependencyInjection;
using Shared.Core.CQRS;
using Shared.Core.Domain;
using ItemCmd = Inventory.Application.Items.Commands;
using ItemQry = Inventory.Application.Items.Queries;

namespace Inventory.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDispatcher, Dispatcher>();

        services.AddScoped<ICommandHandler<ItemCmd.CreateInventoryItemCommand, Result<ItemCmd.InventoryItemResponse>>, ItemCmd.CreateInventoryItemHandler>();
        services.AddScoped<ICommandHandler<ItemCmd.ReceiveStockCommand, Result<ItemCmd.InventoryItemResponse>>, ItemCmd.ReceiveStockHandler>();
        services.AddScoped<ICommandHandler<ItemCmd.AdjustStockCommand, Result<ItemCmd.InventoryItemResponse>>, ItemCmd.AdjustStockHandler>();

        services.AddScoped<IQueryHandler<ItemQry.GetAllInventoryQuery, IReadOnlyList<ItemQry.InventoryItemResponse>>, ItemQry.GetAllInventoryHandler>();
        services.AddScoped<IQueryHandler<ItemQry.GetInventoryByProductIdQuery, ItemQry.InventoryItemResponse?>, ItemQry.GetInventoryByProductIdHandler>();
        services.AddScoped<IQueryHandler<ItemQry.CheckAvailabilityQuery, Shared.Contracts.StockCheckResponse>, ItemQry.CheckAvailabilityHandler>();

        return services;
    }
}
