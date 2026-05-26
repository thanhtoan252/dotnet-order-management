using Notifications.Domain.Entities;

namespace Notifications.Application.Templates.Mappers;

internal static class TemplateMapper
{
    internal static TemplateResponse ToResponse(this NotificationTemplate t)
    {
        return new TemplateResponse(t.Id, t.Type, t.Title, t.BodyTemplate, t.IsActive, t.UpdatedAt);
    }
}
