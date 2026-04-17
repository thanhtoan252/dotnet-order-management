using Microsoft.EntityFrameworkCore;

namespace Order.Infrastructure.Outbox;

public static class OutboxEntityConfiguration
{
    public static void ApplyOutboxConfiguration(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(b =>
        {
            b.ToTable("OutboxMessages");
            b.HasKey(m => m.Id);
            b.HasIndex(m => m.ProcessedAt).HasFilter("ProcessedAt IS NULL");
        });

        modelBuilder.Entity<ProcessedMessage>(b =>
        {
            b.ToTable("ProcessedMessages");
            b.HasKey(m => m.EventId);
        });
    }
}
