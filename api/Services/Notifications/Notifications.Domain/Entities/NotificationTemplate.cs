using Notifications.Domain.Enums;
using Shared.Core.Domain;

namespace Notifications.Domain.Entities;

public sealed class NotificationTemplate : BaseEntity
{
    public NotificationType Type { get; private set; }
    public string Title { get; private set; } = null!;
    public string BodyTemplate { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;

    private NotificationTemplate() { }

    public static NotificationTemplate Create(NotificationType type, string title, string bodyTemplate)
    {
        return new NotificationTemplate
        {
            Type = type,
            Title = title,
            BodyTemplate = bodyTemplate,
            IsActive = true
        };
    }

    public void Update(string title, string bodyTemplate, bool isActive)
    {
        Title = title;
        BodyTemplate = bodyTemplate;
        IsActive = isActive;
    }
}
