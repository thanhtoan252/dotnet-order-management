namespace Notifications.Domain.Enums;

public enum NotificationType
{
    OrderPlaced = 0,
    OrderConfirmed = 1,
    OrderCancelled = 2,
    StockReservationFailed = 3,
    ProductCreated = 10,
    ProductRenamed = 11,
    ProductDeleted = 12,
    AdminBroadcast = 100
}
