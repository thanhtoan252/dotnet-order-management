using Notifications.Domain.Enums;

namespace Notifications.Application.Templates.Mappers;

public sealed record TemplateResponse(
    Guid Id,
    NotificationType Type,
    string Title,
    string BodyTemplate,
    bool IsActive,
    DateTime UpdatedAt);
