using Notifications.Domain.Entities;
using Notifications.Domain.Enums;

namespace Notifications.Application.Templates.Seeding;

public static class DefaultTemplates
{
    public static IReadOnlyList<NotificationTemplate> All()
    {
        return
        [
            NotificationTemplate.Create(NotificationType.OrderPlaced,
                "Order placed",
                "Your order {OrderNumber} was placed and is awaiting stock reservation."),

            NotificationTemplate.Create(NotificationType.OrderConfirmed,
                "Order confirmed",
                "Your order {OrderNumber} has been confirmed. Stock is reserved."),

            NotificationTemplate.Create(NotificationType.OrderCancelled,
                "Order cancelled",
                "Your order {OrderNumber} was cancelled. Reason: {Reason}"),

            NotificationTemplate.Create(NotificationType.StockReservationFailed,
                "Order rejected",
                "Your order could not be fulfilled. Reason: {Reason}"),

            NotificationTemplate.Create(NotificationType.ProductCreated,
                "New product added",
                "Product {Name} (SKU {Sku}) was created with initial stock {InitialStockQuantity}."),

            NotificationTemplate.Create(NotificationType.ProductRenamed,
                "Product renamed",
                "Product {ProductId} was renamed to {NewName}."),

            NotificationTemplate.Create(NotificationType.ProductDeleted,
                "Product deleted",
                "Product {ProductId} was deleted."),

            NotificationTemplate.Create(NotificationType.AdminBroadcast,
                "Announcement",
                "{Message}")
        ];
    }
}
