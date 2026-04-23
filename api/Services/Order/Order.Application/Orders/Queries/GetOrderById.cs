using Order.Application.Orders.Mappers;
using Order.Domain;
using Order.Domain.Repositories;
using Shared.Core.CQRS;
using Shared.Core.Domain;

namespace Order.Application.Orders.Queries;

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
