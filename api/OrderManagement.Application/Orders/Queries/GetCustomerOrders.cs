using OrderManagement.Application.Common.Interfaces;
using OrderManagement.Application.Orders.Mappers;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Application.Orders.Queries;

public record GetCustomerOrdersQuery(Guid CustomerId, int Page = 1, int PageSize = 20)
    : IQuery<IReadOnlyList<OrderResponse>>;

public class GetCustomerOrdersHandler(IOrderRepository orderRepo)
    : IQueryHandler<GetCustomerOrdersQuery, IReadOnlyList<OrderResponse>>
{
    public async Task<IReadOnlyList<OrderResponse>> HandleAsync(GetCustomerOrdersQuery query, CancellationToken ct)
    {
        var orders = await orderRepo.GetByCustomerIdAsync(query.CustomerId, query.Page, query.PageSize, ct);
        return orders.Select(o => o.ToQueryResponse()).ToList();
    }
}
