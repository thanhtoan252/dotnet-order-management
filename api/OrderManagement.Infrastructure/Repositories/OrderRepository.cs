using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Repositories;
using OrderManagement.Infrastructure.Data;

namespace OrderManagement.Infrastructure.Repositories;

public class OrderRepository(OrderDbContext db) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Orders.AsNoTracking().SingleOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Orders
            .Include(o => o.Items)
            .SingleOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId, int page, int pageSize,
        CancellationToken ct = default)
    {
        return await db.Orders
            .Include(o => o.Items)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Order>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        return await db.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Orders.AnyAsync(o => o.Id == id, ct);
    }

    public void Add(Order order)
    {
        db.Orders.Add(order);
    }

    public void Update(Order order)
    {
        db.Orders.Update(order);
    }
}