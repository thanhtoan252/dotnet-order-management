using OrderManagement.Domain.Entities;

namespace OrderManagement.Domain.Repositories;

/// <summary>Repository interface for Order aggregate — lives in Domain layer.</summary>
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId, int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    void Add(Order order);
    void Update(Order order);
}