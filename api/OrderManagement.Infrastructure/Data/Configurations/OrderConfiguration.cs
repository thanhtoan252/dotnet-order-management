using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.RowVersion).IsRowVersion().IsConcurrencyToken();

        builder.Property(o => o.OrderNumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(o => o.OrderNumber).IsUnique();

        // Owned type: Address (stored as flat columns in same table)
        builder.OwnsOne(o => o.ShippingAddress, a =>
        {
            a.Property(x => x.Street).HasColumnName("ShippingStreet").HasMaxLength(200).IsRequired();
            a.Property(x => x.City).HasColumnName("ShippingCity").HasMaxLength(100).IsRequired();
            a.Property(x => x.Province).HasColumnName("ShippingProvince").HasMaxLength(100).IsRequired();
            a.Property(x => x.ZipCode).HasColumnName("ShippingZipCode").HasMaxLength(20).IsRequired();
        });

        // Owned type: Money
        builder.OwnsOne(o => o.TotalAmount, m =>
        {
            m.Property(x => x.Amount).HasColumnName("TotalAmount").HasColumnType("decimal(18,2)").IsRequired();
            m.Property(x => x.Currency).HasColumnName("TotalCurrency").HasMaxLength(3).IsRequired();
        });

        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(o => o.Notes).HasMaxLength(2000);

        builder.HasIndex(o => o.CustomerId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.CreatedAt);

        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}