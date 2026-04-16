using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.Entities;

namespace Order.Infrastructure.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.RowVersion).IsRowVersion().IsConcurrencyToken();
        builder.Property(i => i.ProductName).HasMaxLength(200).IsRequired();

        builder.OwnsOne(i => i.UnitPrice, m =>
        {
            m.Property(x => x.Amount).HasColumnName("UnitPrice").HasColumnType("decimal(18,2)").IsRequired();
            m.Property(x => x.Currency).HasColumnName("Currency").HasMaxLength(3).IsRequired();
        });

        builder.HasIndex(i => i.ProductId);
    }
}
