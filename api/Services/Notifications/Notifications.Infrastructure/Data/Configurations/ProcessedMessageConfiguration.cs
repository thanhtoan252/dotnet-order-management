using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notifications.Infrastructure.Persistence;

namespace Notifications.Infrastructure.Data.Configurations;

public class ProcessedMessageConfiguration : IEntityTypeConfiguration<ProcessedMessage>
{
    public void Configure(EntityTypeBuilder<ProcessedMessage> builder)
    {
        builder.ToTable("ProcessedMessages");
        builder.HasKey(m => m.EventId);
        builder.Property(m => m.EventType).HasMaxLength(120).IsRequired();
        builder.Property(m => m.ProcessedAt).IsRequired();
    }
}
