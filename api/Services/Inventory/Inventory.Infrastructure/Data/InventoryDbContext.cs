using Inventory.Domain.Entities;
using Inventory.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Domain;

namespace Inventory.Infrastructure.Data;

public class InventoryDbContext(DbContextOptions<InventoryDbContext> options, TimeProvider timeProvider)
    : DbContext(options)
{
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);

        modelBuilder.Entity<InventoryItem>().HasQueryFilter(i => !i.IsDeleted);

        modelBuilder.ApplyOutboxConfiguration();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>().Where(e => e.State is EntityState.Modified))
        {
            entry.Entity.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        foreach (var aggregate in ChangeTracker.Entries<AggregateRoot>()
                     .Select(e => e.Entity)
                     .Where(a => a.DomainEvents.Count != 0))
        {
            aggregate.ClearDomainEvents();
        }

        return result;
    }
}
