namespace Inventory.Application.Items.Models;

public sealed record InventoryItemResult(
    Guid ProductId,
    string Sku,
    string ProductName,
    int OnHand,
    int Reserved,
    int Available);
