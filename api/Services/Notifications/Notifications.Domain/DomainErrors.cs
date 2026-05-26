using Shared.Core.Domain;

namespace Notifications.Domain;

public static class DomainErrors
{
    public static class Notification
    {
        public static Error NotFound(Guid id)
            => new("Notification.NotFound", $"Notification {id} not found.", ErrorType.NotFound);

        public static Error NotOwned() => new("Notification.NotOwned",
            "Notification does not belong to the current user.",
            ErrorType.Forbidden);
    }

    public static class Template
    {
        public static Error NotFound(Guid id)
            => new("Template.NotFound", $"Template {id} not found.", ErrorType.NotFound);
    }
}
