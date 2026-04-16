using Microsoft.EntityFrameworkCore;
using Order.Domain.Entities;
using Order.Domain.Repositories;
using Order.Infrastructure.Data;

namespace Order.Infrastructure.Repositories;

public class OrderRepository(OrderDbContext db) : IOrderRepository
{
    public async Task<OrderAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Orders.AsNoTracking().SingleOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<OrderAggregate?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Orders
            .Include(o => o.Items)
            .SingleOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<IReadOnlyList<OrderAggregate>> GetByCustomerIdAsync(Guid customerId, int page, int pageSize,
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

    public async Task<IReadOnlyList<OrderAggregate>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
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

    public void Add(OrderAggregate order)
    {
        db.Orders.Add(order);
    }

    public void Update(OrderAggregate order)
    {
        db.Orders.Update(order);
    }
}
