using Microsoft.EntityFrameworkCore;
using Order.Application.Abstractions;
using Order.Application.Orders.Mappers;
using Order.Application.Orders.Models;
using Shared.Core.CQRS;

namespace Order.Application.Orders.Queries;

public record GetAllOrdersQuery(int Page = 1, int PageSize = 100)
    : IQuery<IReadOnlyList<OrderResult>>;

public class GetAllOrdersHandler(IOrderDbContext db)
    : IQueryHandler<GetAllOrdersQuery, IReadOnlyList<OrderResult>>
{
    public async Task<IReadOnlyList<OrderResult>> HandleAsync(GetAllOrdersQuery query, CancellationToken ct)
    {
        var orders = await db.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .AsNoTracking()
            .ToListAsync(ct);

        return orders.Select(o => o.ToResult()).ToList();
    }
}
