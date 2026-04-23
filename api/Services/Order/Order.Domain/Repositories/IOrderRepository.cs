using Order.Domain.Entities;

namespace Order.Domain.Repositories;

public interface IOrderRepository
{
    Task<OrderAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<OrderAggregate?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<OrderAggregate>> GetByCustomerIdAsync(Guid customerId, int page, int pageSize,
        CancellationToken ct = default);

    Task<IReadOnlyList<OrderAggregate>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    void Add(OrderAggregate order);
    void Update(OrderAggregate order);
}
