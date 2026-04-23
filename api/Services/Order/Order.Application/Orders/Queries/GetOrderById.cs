using Microsoft.EntityFrameworkCore;
using Order.Application.Abstractions;
using Order.Application.Orders.Mappers;
using Order.Domain;
using Shared.Core.CQRS;
using Shared.Core.Domain;

namespace Order.Application.Orders.Queries;

public record GetOrderByIdQuery(Guid OrderId)
    : IQuery<Result<OrderResponse>>;

public class GetOrderByIdHandler(IOrderDbContext db)
    : IQueryHandler<GetOrderByIdQuery, Result<OrderResponse>>
{
    public async Task<Result<OrderResponse>> HandleAsync(GetOrderByIdQuery query, CancellationToken ct)
    {
        var order = await db.Orders
            .Include(o => o.Items)
            .SingleOrDefaultAsync(o => o.Id == query.OrderId, ct);

        if (order is null)
        {
            return DomainErrors.Order.NotFound(query.OrderId);
        }

        return order.ToQueryResponse();
    }
}
