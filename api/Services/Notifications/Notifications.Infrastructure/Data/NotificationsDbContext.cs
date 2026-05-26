using Microsoft.EntityFrameworkCore;
using Notifications.Application.Abstractions;
using Notifications.Domain.Entities;
using Notifications.Infrastructure.Persistence;
using Shared.Core.Domain;

namespace Notifications.Infrastructure.Data;

public class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options, TimeProvider timeProvider)
    : DbContext(options), INotificationDbContext
{
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationTemplate> Templates => Set<NotificationTemplate>();
    public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationsDbContext).Assembly);

        modelBuilder.Entity<Notification>().HasQueryFilter(n => !n.IsDeleted);
        modelBuilder.Entity<NotificationTemplate>().HasQueryFilter(t => !t.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>().Where(e => e.State is EntityState.Modified))
        {
            entry.Entity.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
