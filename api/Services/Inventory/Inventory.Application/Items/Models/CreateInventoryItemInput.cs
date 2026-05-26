namespace Inventory.Application.Items.Models;

public sealed record CreateInventoryItemInput(Guid ProductId, string Sku, string ProductName, int InitialQuantity);
