using Microsoft.EntityFrameworkCore;
using Order.Domain.Entities;

namespace Order.Application.Abstractions;

public interface IOrderDbContext
{
    DbSet<OrderAggregate> Orders { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
