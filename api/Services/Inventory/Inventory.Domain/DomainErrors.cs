using Shared.Core.Domain;

namespace Inventory.Domain;

public static class DomainErrors
{
    public static class InventoryItem
    {
        public static readonly Error InvalidQuantity =
            new("InventoryItem.InvalidQuantity", "Quantity must be positive.");

        public static readonly Error InvalidSku =
            new("InventoryItem.InvalidSku", "SKU cannot be empty.");

        public static readonly Error InvalidProductName =
            new("InventoryItem.InvalidProductName", "Product name cannot be empty.");

        public static Error NotFound(Guid productId)
        {
            return new Error("InventoryItem.NotFound", $"Inventory for product {productId} not found.");
        }

        public static Error AlreadyExists(Guid productId)
        {
            return new Error("InventoryItem.AlreadyExists", $"Inventory for product {productId} already exists.");
        }

        public static Error InsufficientStock(string productName, int available, int requested)
        {
            return new Error("InventoryItem.InsufficientStock",
                $"Insufficient stock for '{productName}'. Available: {available}, Requested: {requested}.");
        }

        public static Error InsufficientReserved(string productName, int reserved, int requested)
        {
            return new Error("InventoryItem.InsufficientReserved",
                $"Cannot release/commit more than reserved for '{productName}'. Reserved: {reserved}, Requested: {requested}.");
        }
    }
}
