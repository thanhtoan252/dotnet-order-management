using Notifications.Domain.Enums;
using Shared.Core.Domain;

namespace Notifications.Domain.Entities;

public sealed class Notification : BaseEntity
{
    public Guid? UserId { get; private set; }
    public Guid? OrderId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Title { get; private set; } = null!;
    public string Body { get; private set; } = null!;
    public NotificationStatus Status { get; private set; } = NotificationStatus.Unread;
    public string? Metadata { get; private set; }
    public DateTime? ReadAt { get; private set; }

    private Notification() { }

    public static Notification ForUser(Guid userId, NotificationType type, string title, string body,
        Guid? orderId = null, string? metadata = null)
    {
        return new Notification
        {
            UserId = userId,
            OrderId = orderId,
            Type = type,
            Title = title,
            Body = body,
            Metadata = metadata
        };
    }

    public static Notification ForAdmins(NotificationType type, string title, string body,
        string? metadata = null)
    {
        return new Notification
        {
            UserId = null,
            Type = type,
            Title = title,
            Body = body,
            Metadata = metadata
        };
    }

    public static Notification Broadcast(string title, string body, string? metadata = null)
    {
        return new Notification
        {
            UserId = null,
            Type = NotificationType.AdminBroadcast,
            Title = title,
            Body = body,
            Metadata = metadata
        };
    }

    public void MarkAsRead(DateTime nowUtc)
    {
        if (Status == NotificationStatus.Read)
        {
            return;
        }

        Status = NotificationStatus.Read;
        ReadAt = nowUtc;
    }
}
