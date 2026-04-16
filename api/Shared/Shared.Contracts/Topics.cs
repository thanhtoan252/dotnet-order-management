namespace Shared.Contracts;

public static class Topics
{
    public const string OrderPlaced = "order.placed";
    public const string OrderCancelled = "order.cancelled";
    public const string StockReserved = "catalog.stock-reserved";
    public const string StockReservationFailed = "catalog.stock-reservation-failed";
    public const string ProductPriceChanged = "catalog.product-price-changed";
}
