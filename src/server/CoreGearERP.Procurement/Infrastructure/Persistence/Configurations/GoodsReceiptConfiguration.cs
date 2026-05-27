using CoreGearERP.Procurement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGearERP.Procurement.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for GoodsReceipt.
/// </summary>
public class GoodsReceiptConfiguration : IEntityTypeConfiguration<GoodsReceipt>
{
    public void Configure(EntityTypeBuilder<GoodsReceipt> builder)
    {
        builder.ToTable("GoodsReceipts");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.PurchaseOrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.WarehouseId)
            .IsRequired();

        builder.Property(r => r.ReceivedAt)
            .IsRequired();

        builder.Property(r => r.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.TenantId)
            .IsRequired();

        builder.HasIndex(r => r.PurchaseOrderId);
        builder.HasIndex(r => r.TenantId);

        builder.HasMany(r => r.Lines)
            .WithOne()
            .HasForeignKey(l => l.GoodsReceiptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}