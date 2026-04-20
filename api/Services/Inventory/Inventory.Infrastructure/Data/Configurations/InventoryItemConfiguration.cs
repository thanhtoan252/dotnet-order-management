using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Data.Configurations;

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("InventoryItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.RowVersion).IsRowVersion().IsConcurrencyToken();

        builder.Property(i => i.ProductId).IsRequired();
        builder.HasIndex(i => i.ProductId).IsUnique();

        builder.Property(i => i.Sku).HasMaxLength(50).IsRequired();
        builder.HasIndex(i => i.Sku);

        builder.Property(i => i.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(i => i.OnHand).IsRequired();
        builder.Property(i => i.Reserved).IsRequired();

        builder.Ignore(i => i.Available);
    }
}
