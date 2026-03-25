using OrderManagement.Application.Common.Interfaces;
using OrderManagement.Application.Orders.Mappers;
using OrderManagement.Domain.Common;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Application.Orders.Queries;

public record GetOrderByIdQuery(Guid OrderId)
    : IQuery<Result<OrderResponse>>;

public class GetOrderByIdHandler(IOrderRepository orderRepo)
    : IQueryHandler<GetOrderByIdQuery, Result<OrderResponse>>
{
    public async Task<Result<OrderResponse>> HandleAsync(GetOrderByIdQuery query, CancellationToken ct)
    {
        var order = await orderRepo.GetByIdWithItemsAsync(query.OrderId, ct);
        if (order is null)
        {
            return DomainErrors.Order.NotFound(query.OrderId);
        }

        return order.ToQueryResponse();
    }
}
