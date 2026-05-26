using Notifications.Domain.Enums;

namespace Notifications.Application.Notifications.Mappers;

public sealed record NotificationResponse(
    Guid Id,
    Guid? UserId,
    Guid? OrderId,
    NotificationType Type,
    string Title,
    string Body,
    NotificationStatus Status,
    string? Metadata,
    DateTime CreatedAt,
    DateTime? ReadAt);
