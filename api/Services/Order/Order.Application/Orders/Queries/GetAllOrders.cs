using Microsoft.EntityFrameworkCore;
using Order.Application.Abstractions;
using Order.Application.Orders.Mappers;
using Shared.Core.CQRS;

namespace Order.Application.Orders.Queries;

public record GetAllOrdersQuery(int Page = 1, int PageSize = 100)
    : IQuery<IReadOnlyList<OrderResponse>>;

public class GetAllOrdersHandler(IOrderDbContext db)
    : IQueryHandler<GetAllOrdersQuery, IReadOnlyList<OrderResponse>>
{
    public async Task<IReadOnlyList<OrderResponse>> HandleAsync(GetAllOrdersQuery query, CancellationToken ct)
    {
        var orders = await db.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .AsNoTracking()
            .ToListAsync(ct);

        return orders.Select(o => o.ToQueryResponse()).ToList();
    }
}
