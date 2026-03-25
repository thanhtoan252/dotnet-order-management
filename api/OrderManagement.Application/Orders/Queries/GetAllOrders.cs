using OrderManagement.Application.Common.Interfaces;
using OrderManagement.Application.Orders.Mappers;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Application.Orders.Queries;

public record GetAllOrdersQuery(int Page = 1, int PageSize = 100)
    : IQuery<IReadOnlyList<OrderResponse>>;

public class GetAllOrdersHandler(IOrderRepository orderRepo)
    : IQueryHandler<GetAllOrdersQuery, IReadOnlyList<OrderResponse>>
{
    public async Task<IReadOnlyList<OrderResponse>> HandleAsync(GetAllOrdersQuery query, CancellationToken ct)
    {
        var orders = await orderRepo.GetAllAsync(query.Page, query.PageSize, ct);
        return orders.Select(o => o.ToQueryResponse()).ToList();
    }
}
