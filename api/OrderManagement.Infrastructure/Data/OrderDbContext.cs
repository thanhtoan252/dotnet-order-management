using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using Serilog;

namespace OrderManagement.Infrastructure.Data;

public class OrderDbContext(DbContextOptions<OrderDbContext> options, TimeProvider timeProvider) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);

        // Global soft-delete filters
        modelBuilder.Entity<Order>().HasQueryFilter(o => !o.IsDeleted);
        modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<OrderItem>().HasQueryFilter(i => !i.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>().Where(e => e.State is EntityState.Modified))
        {
            entry.Entity.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
        }

        // Collect domain events before saving (entities will be cleared after)
        var aggregates = ChangeTracker.Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count != 0)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        // Dispatch domain events (fire-and-forget log; replace with event bus in production)
        foreach (var aggregate in aggregates)
        {
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                Log.Information("Domain Event dispatched: {EventType} {@Event}", domainEvent.GetType().Name,
                    domainEvent);
            }

            aggregate.ClearDomainEvents();
        }

        return result;
    }
}
