using Microsoft.EntityFrameworkCore;
using Notifications.Application.Abstractions;
using Notifications.Application.Notifications.Mappers;
using Notifications.Domain.Enums;
using Shared.Core.CQRS;

namespace Notifications.Application.Notifications.Queries;

public sealed record GetMyNotificationsQuery(
    Guid UserId,
    bool IsAdmin,
    NotificationStatus? Status,
    int Page,
    int PageSize)
    : IQuery<IReadOnlyList<NotificationResponse>>;

public sealed class GetMyNotificationsHandler(INotificationDbContext db)
    : IQueryHandler<GetMyNotificationsQuery, IReadOnlyList<NotificationResponse>>
{
    public async Task<IReadOnlyList<NotificationResponse>> HandleAsync(
        GetMyNotificationsQuery query, CancellationToken ct = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 20 : Math.Min(query.PageSize, 100);

        var q = db.Notifications.AsNoTracking().Where(n =>
            n.UserId == query.UserId
            || n.Type == NotificationType.AdminBroadcast
            || (query.IsAdmin && n.UserId == null && n.Type != NotificationType.AdminBroadcast));

        if (query.Status is { } status)
        {
            q = q.Where(n => n.Status == status);
        }

        var items = await q
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return items.Select(n => n.ToResponse()).ToList();
    }
}
