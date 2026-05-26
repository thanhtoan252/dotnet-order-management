using Microsoft.Extensions.DependencyInjection;
using Shared.Core.CQRS;
using Shared.Core.Domain;
using Inventory.Application.Items.Models;
using ItemCmd = Inventory.Application.Items.Commands;
using ItemQry = Inventory.Application.Items.Queries;

namespace Inventory.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDispatcher, Dispatcher>();

        services.AddScoped<
            ICommandHandler<ItemCmd.CreateInventoryItemCommand, Result<InventoryItemResult>>,
            ItemCmd.CreateInventoryItemHandler>();
        services.AddScoped<
            ICommandHandler<ItemCmd.ReceiveStockCommand, Result<InventoryItemResult>>,
            ItemCmd.ReceiveStockHandler>();
        services.AddScoped<
            ICommandHandler<ItemCmd.AdjustStockCommand, Result<InventoryItemResult>>,
            ItemCmd.AdjustStockHandler>();

        services.AddScoped<
            IQueryHandler<ItemQry.GetAllInventoryQuery, IReadOnlyList<InventoryItemResult>>,
            ItemQry.GetAllInventoryHandler>();
        services.AddScoped<
            IQueryHandler<ItemQry.GetInventoryByProductIdQuery, InventoryItemResult?>,
            ItemQry.GetInventoryByProductIdHandler>();
        services.AddScoped<
            IQueryHandler<ItemQry.CheckAvailabilityQuery, Shared.Contracts.StockCheckResponse>,
            ItemQry.CheckAvailabilityHandler>();

        return services;
    }
}
