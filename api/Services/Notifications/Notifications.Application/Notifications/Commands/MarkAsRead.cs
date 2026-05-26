using Microsoft.EntityFrameworkCore;
using Notifications.Application.Abstractions;
using Notifications.Domain;
using Notifications.Domain.Enums;
using Shared.Core.CQRS;
using Shared.Core.Domain;

namespace Notifications.Application.Notifications.Commands;

public sealed record MarkAsReadCommand(Guid NotificationId, Guid UserId, bool IsAdmin)
    : ICommand<Result>;

public sealed class MarkAsReadHandler(INotificationDbContext db, TimeProvider clock)
    : ICommandHandler<MarkAsReadCommand, Result>
{
    public async Task<Result> HandleAsync(MarkAsReadCommand command, CancellationToken ct = default)
    {
        var notification = await db.Notifications.SingleOrDefaultAsync(n => n.Id == command.NotificationId, ct);
        if (notification is null)
        {
            return DomainErrors.Notification.NotFound(command.NotificationId);
        }

        var canRead =
            notification.UserId == command.UserId
            || notification.Type == NotificationType.AdminBroadcast
            || (command.IsAdmin && notification.UserId == null && notification.Type != NotificationType.AdminBroadcast);

        if (!canRead)
        {
            return DomainErrors.Notification.NotOwned();
        }

        notification.MarkAsRead(clock.GetUtcNow().UtcDateTime);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
