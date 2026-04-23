using Microsoft.EntityFrameworkCore;
using Order.Application.Abstractions;
using Order.Application.Orders.Mappers;
using Shared.Core.CQRS;

namespace Order.Application.Orders.Queries;

public record GetCustomerOrdersQuery(Guid CustomerId, int Page = 1, int PageSize = 20)
    : IQuery<IReadOnlyList<OrderResponse>>;

public class GetCustomerOrdersHandler(IOrderDbContext db)
    : IQueryHandler<GetCustomerOrdersQuery, IReadOnlyList<OrderResponse>>
{
    public async Task<IReadOnlyList<OrderResponse>> HandleAsync(GetCustomerOrdersQuery query, CancellationToken ct)
    {
        var orders = await db.Orders
            .Include(o => o.Items)
            .Where(o => o.CustomerId == query.CustomerId)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .AsNoTracking()
            .ToListAsync(ct);

        return orders.Select(o => o.ToQueryResponse()).ToList();
    }
}
