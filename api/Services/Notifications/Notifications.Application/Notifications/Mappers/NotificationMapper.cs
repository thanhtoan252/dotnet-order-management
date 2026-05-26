using Notifications.Domain.Entities;

namespace Notifications.Application.Notifications.Mappers;

internal static class NotificationMapper
{
    internal static NotificationResponse ToResponse(this Notification n)
    {
        return new NotificationResponse(
            n.Id,
            n.UserId,
            n.OrderId,
            n.Type,
            n.Title,
            n.Body,
            n.Status,
            n.Metadata,
            n.CreatedAt,
            n.ReadAt);
    }
}
