using Microsoft.EntityFrameworkCore;
using Order.Application.Abstractions;
using Order.Application.Orders.Mappers;
using Order.Application.Orders.Models;
using Shared.Core.CQRS;

namespace Order.Application.Orders.Queries;

public record GetCustomerOrdersQuery(Guid CustomerId, int Page = 1, int PageSize = 20)
    : IQuery<IReadOnlyList<OrderResult>>;

public class GetCustomerOrdersHandler(IOrderDbContext db)
    : IQueryHandler<GetCustomerOrdersQuery, IReadOnlyList<OrderResult>>
{
    public async Task<IReadOnlyList<OrderResult>> HandleAsync(GetCustomerOrdersQuery query, CancellationToken ct)
    {
        var orders = await db.Orders
            .Include(o => o.Items)
            .Where(o => o.CustomerId == query.CustomerId)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .AsNoTracking()
            .ToListAsync(ct);

        return orders.Select(o => o.ToResult()).ToList();
    }
}
