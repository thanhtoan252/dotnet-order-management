namespace Shared.Contracts;

public static class Topics
{
    public const string OrderPlaced = "order.placed";
    public const string OrderCancelled = "order.cancelled";
    public const string StockReserved = "inventory.stock-reserved";
    public const string StockReservationFailed = "inventory.stock-reservation-failed";
    public const string ProductCreated = "catalog.product-created";
    public const string ProductDeleted = "catalog.product-deleted";
    public const string ProductRenamed = "catalog.product-renamed";
}
