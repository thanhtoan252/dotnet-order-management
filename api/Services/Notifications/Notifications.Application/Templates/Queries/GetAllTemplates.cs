using Microsoft.EntityFrameworkCore;
using Notifications.Application.Abstractions;
using Notifications.Application.Templates.Mappers;
using Shared.Core.CQRS;

namespace Notifications.Application.Templates.Queries;

public sealed record GetAllTemplatesQuery : IQuery<IReadOnlyList<TemplateResponse>>;

public sealed class GetAllTemplatesHandler(INotificationDbContext db)
    : IQueryHandler<GetAllTemplatesQuery, IReadOnlyList<TemplateResponse>>
{
    public async Task<IReadOnlyList<TemplateResponse>> HandleAsync(GetAllTemplatesQuery query,
        CancellationToken ct = default)
    {
        var items = await db.Templates.AsNoTracking().OrderBy(t => t.Type).ToListAsync(ct);
        return items.Select(t => t.ToResponse()).ToList();
    }
}
