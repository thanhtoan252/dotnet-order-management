using Microsoft.EntityFrameworkCore;
using Notifications.Application.Abstractions;
using Notifications.Application.Templates.Rendering;
using Notifications.Domain.Entities;
using Notifications.Domain.Enums;

namespace Notifications.Application.EventHandlers;

internal static class NotificationFactory
{
    internal static async Task<(string Title, string Body)?> RenderAsync(
        INotificationDbContext db,
        NotificationType type,
        IReadOnlyDictionary<string, string?> values,
        CancellationToken ct)
    {
        var template = await db.Templates
            .AsNoTracking()
            .Where(t => t.Type == type && t.IsActive)
            .FirstOrDefaultAsync(ct);

        if (template is null)
        {
            return null;
        }

        return (template.Title, TemplateRenderer.Render(template.BodyTemplate, values));
    }

    internal static async Task<Guid?> ResolveUserIdForOrderAsync(
        INotificationDbContext db, Guid orderId, CancellationToken ct)
    {
        return await db.Notifications
            .AsNoTracking()
            .Where(n => n.OrderId == orderId && n.Type == NotificationType.OrderPlaced)
            .Select(n => n.UserId)
            .FirstOrDefaultAsync(ct);
    }
}
