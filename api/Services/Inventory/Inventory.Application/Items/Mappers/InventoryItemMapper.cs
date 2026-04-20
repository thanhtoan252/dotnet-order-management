using Inventory.Domain.Entities;
using CmdDto = Inventory.Application.Items.Commands;
using QryDto = Inventory.Application.Items.Queries;

namespace Inventory.Application.Items.Mappers;

internal static class InventoryItemMapper
{
    internal static CmdDto.InventoryItemResponse ToCommandResponse(this InventoryItem item)
    {
        return new CmdDto.InventoryItemResponse
        {
            ProductId = item.ProductId,
            Sku = item.Sku,
            ProductName = item.ProductName,
            OnHand = item.OnHand,
            Reserved = item.Reserved,
            Available = item.Available
        };
    }

    internal static QryDto.InventoryItemResponse ToQueryResponse(this InventoryItem item)
    {
        return new QryDto.InventoryItemResponse
        {
            ProductId = item.ProductId,
            Sku = item.Sku,
            ProductName = item.ProductName,
            OnHand = item.OnHand,
            Reserved = item.Reserved,
            Available = item.Available
        };
    }
}
