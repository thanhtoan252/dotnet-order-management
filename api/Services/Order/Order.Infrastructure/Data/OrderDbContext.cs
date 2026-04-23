using Microsoft.EntityFrameworkCore;
using Order.Application.Abstractions;
using Order.Domain.Entities;
using Order.Infrastructure.Outbox;
using Shared.Core.Domain;

namespace Order.Infrastructure.Data;

public class OrderDbContext(DbContextOptions<OrderDbContext> options, TimeProvider timeProvider)
    : DbContext(options), IOrderDbContext
{
    public DbSet<OrderAggregate> Orders => Set<OrderAggregate>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);

        // Global soft-delete filters
        modelBuilder.Entity<OrderAggregate>().HasQueryFilter(o => !o.IsDeleted);
        modelBuilder.Entity<OrderItem>().HasQueryFilter(i => !i.IsDeleted);

        modelBuilder.ApplyOutboxConfiguration();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>().Where(e => e.State is EntityState.Modified))
        {
            entry.Entity.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        // Clear domain events after save — no handlers yet, just housekeeping
        foreach (var aggregate in ChangeTracker.Entries<AggregateRoot>()
                     .Select(e => e.Entity)
                     .Where(a => a.DomainEvents.Count != 0))
        {
            aggregate.ClearDomainEvents();
        }

        return result;
    }
}
