using Inventory.Domain.Entities;
using Inventory.Application.Items.Models;

namespace Inventory.Application.Items.Mappers;

internal static class InventoryItemMapper
{
    internal static InventoryItemResult ToResult(this InventoryItem item)
    {
        return new InventoryItemResult(
            item.ProductId,
            item.Sku,
            item.ProductName,
            item.OnHand,
            item.Reserved,
            item.Available);
    }
}
