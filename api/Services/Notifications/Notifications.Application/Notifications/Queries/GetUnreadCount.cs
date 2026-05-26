using Microsoft.EntityFrameworkCore;
using Notifications.Application.Abstractions;
using Notifications.Domain.Enums;
using Shared.Core.CQRS;

namespace Notifications.Application.Notifications.Queries;

public sealed record GetUnreadCountQuery(Guid UserId, bool IsAdmin) : IQuery<int>;

public sealed class GetUnreadCountHandler(INotificationDbContext db)
    : IQueryHandler<GetUnreadCountQuery, int>
{
    public async Task<int> HandleAsync(GetUnreadCountQuery query, CancellationToken ct = default)
    {
        return await db.Notifications
            .AsNoTracking()
            .Where(n => n.Status == NotificationStatus.Unread)
            .Where(n =>
                n.UserId == query.UserId
                || n.Type == NotificationType.AdminBroadcast
                || (query.IsAdmin && n.UserId == null && n.Type != NotificationType.AdminBroadcast))
            .CountAsync(ct);
    }
}
