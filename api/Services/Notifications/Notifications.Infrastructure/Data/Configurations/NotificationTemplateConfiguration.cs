using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notifications.Domain.Entities;

namespace Notifications.Infrastructure.Data.Configurations;

public class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.ToTable("NotificationTemplates");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.RowVersion).IsRowVersion().IsConcurrencyToken();

        builder.Property(t => t.Type).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(t => t.Title).HasMaxLength(200).IsRequired();
        builder.Property(t => t.BodyTemplate).HasMaxLength(2000).IsRequired();
        builder.Property(t => t.IsActive).IsRequired();

        builder.HasIndex(t => t.Type).IsUnique();
    }
}
