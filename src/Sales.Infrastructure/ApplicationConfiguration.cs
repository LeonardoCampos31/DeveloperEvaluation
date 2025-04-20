using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sales.Domain.Entities;

namespace Sales.Infrastructure;

public class ApplicationConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.SaleNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.TotalAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(s => s.CustomerId).IsRequired();
        builder.Property(s => s.BranchId).IsRequired();

        builder.HasMany(s => s.Items)
            .WithOne(i => i.Sale)
            .HasForeignKey(i => i.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(s => s.DomainEvents);

        builder.HasIndex(s => s.SaleNumber).IsUnique();
        builder.HasIndex(s => s.SaleDate);
    }
}

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
     public void Configure(EntityTypeBuilder<SaleItem> builder)
     {
         builder.HasKey(i => i.Id);
         builder.Property(i => i.UnitPrice).HasColumnType("decimal(18, 2)");
         builder.Property(i => i.ValueMonetaryTaxApplied).HasColumnType("decimal(18, 2)");
         builder.Property(i => i.Total).HasColumnType("decimal(18, 2)");
         builder.Property(i => i.ProductId).IsRequired();
     }
}

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
     public void Configure(EntityTypeBuilder<Product> builder)
     {
         builder.HasKey(p => p.Id);
         builder.Property(p => p.Title).IsRequired().HasMaxLength(200);
         builder.Property(p => p.Price).HasColumnType("decimal(18, 2)");
         builder.Property(p => p.Description).HasMaxLength(1000);
         builder.Property(p => p.Category).HasMaxLength(100);
     }
}