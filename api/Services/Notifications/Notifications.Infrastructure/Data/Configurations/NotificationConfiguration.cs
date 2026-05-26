using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notifications.Domain.Entities;

namespace Notifications.Infrastructure.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.RowVersion).IsRowVersion().IsConcurrencyToken();

        builder.Property(n => n.UserId);
        builder.Property(n => n.OrderId);

        builder.Property(n => n.Type).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(n => n.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.Property(n => n.Title).HasMaxLength(200).IsRequired();
        builder.Property(n => n.Body).HasMaxLength(2000).IsRequired();
        builder.Property(n => n.Metadata).HasColumnType("nvarchar(max)");

        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => n.OrderId);
        builder.HasIndex(n => new { n.UserId, n.Status });
        builder.HasIndex(n => n.CreatedAt);
    }
}
