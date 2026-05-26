namespace Inventory.Application.Items.Models;

public sealed record AdjustStockInput(int OnHand, string? Reason);
