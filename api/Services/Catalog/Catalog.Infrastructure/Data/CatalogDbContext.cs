using Catalog.Domain.Entities;
using Catalog.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Domain;

namespace Catalog.Infrastructure.Data;

public class CatalogDbContext(DbContextOptions<CatalogDbContext> options, TimeProvider timeProvider)
    : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);

        // Global soft-delete filter
        modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);

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
