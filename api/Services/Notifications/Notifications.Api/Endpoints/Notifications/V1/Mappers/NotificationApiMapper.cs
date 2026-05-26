using AppNotification = Notifications.Application.Notifications.Mappers.NotificationResponse;
using AppTemplate = Notifications.Application.Templates.Mappers.TemplateResponse;

namespace Notifications.Api.Endpoints.Notifications.V1.Mappers;

internal static class NotificationApiMapper
{
    internal static DTOs.NotificationResponse ToDto(this AppNotification notification)
    {
        return new DTOs.NotificationResponse
        {
            Id = notification.Id,
            UserId = notification.UserId,
            OrderId = notification.OrderId,
            Type = Enum.Parse<DTOs.NotificationType>(notification.Type.ToString()),
            Title = notification.Title,
            Body = notification.Body,
            Status = Enum.Parse<DTOs.NotificationStatus>(notification.Status.ToString()),
            Metadata = notification.Metadata,
            CreatedAt = new DateTimeOffset(notification.CreatedAt, TimeSpan.Zero),
            ReadAt = notification.ReadAt is null ? null : new DateTimeOffset(notification.ReadAt.Value, TimeSpan.Zero)
        };
    }

    internal static DTOs.TemplateResponse ToDto(this AppTemplate template)
    {
        return new DTOs.TemplateResponse
        {
            Id = template.Id,
            Type = Enum.Parse<DTOs.NotificationType>(template.Type.ToString()),
            Title = template.Title,
            BodyTemplate = template.BodyTemplate,
            IsActive = template.IsActive,
            UpdatedAt = new DateTimeOffset(template.UpdatedAt, TimeSpan.Zero)
        };
    }
}
