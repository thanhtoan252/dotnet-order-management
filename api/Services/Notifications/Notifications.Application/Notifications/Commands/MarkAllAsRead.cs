using Microsoft.EntityFrameworkCore;
using Notifications.Application.Abstractions;
using Notifications.Domain.Enums;
using Shared.Core.CQRS;
using Shared.Core.Domain;

namespace Notifications.Application.Notifications.Commands;

public sealed record MarkAllAsReadCommand(Guid UserId, bool IsAdmin) : ICommand<Result<int>>;

public sealed class MarkAllAsReadHandler(INotificationDbContext db, TimeProvider clock)
    : ICommandHandler<MarkAllAsReadCommand, Result<int>>
{
    public async Task<Result<int>> HandleAsync(MarkAllAsReadCommand command, CancellationToken ct = default)
    {
        var nowUtc = clock.GetUtcNow().UtcDateTime;

        var rows = await db.Notifications
            .Where(n => n.Status == NotificationStatus.Unread)
            .Where(n =>
                n.UserId == command.UserId
                || n.Type == NotificationType.AdminBroadcast
                || (command.IsAdmin && n.UserId == null && n.Type != NotificationType.AdminBroadcast))
            .ToListAsync(ct);

        foreach (var notification in rows)
        {
            notification.MarkAsRead(nowUtc);
        }

        if (rows.Count > 0)
        {
            await db.SaveChangesAsync(ct);
        }

        return rows.Count;
    }
}
