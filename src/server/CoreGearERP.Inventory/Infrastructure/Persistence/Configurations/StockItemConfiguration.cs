using CoreGearERP.Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGearERP.Inventory.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping configuration for the StockItem entity.
/// </summary>
public class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.ToTable("stock_items");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(s => s.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(s => s.WarehouseId).HasColumnName("warehouse_id").IsRequired();
        builder.Property(s => s.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
        builder.Property(s => s.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(s => s.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(s => s.CreatedBy).HasColumnName("created_by").IsRequired();
        builder.Property(s => s.ModifiedAt).HasColumnName("modified_at").IsRequired();
        builder.Property(s => s.ModifiedBy).HasColumnName("modified_by").IsRequired();
        builder.Property(s => s.ConfirmedAt).HasColumnName("confirmed_at");
        builder.Property(s => s.CompletedAt).HasColumnName("completed_at");
        builder.Property(s => s.CancelledAt).HasColumnName("cancelled_at");

        // Quantity owned value objects stored as two columns each.
        builder.OwnsOne(s => s.QuantityOnHand, q =>
        {
            q.Property(x => x.Value).HasColumnName("qty_on_hand").HasPrecision(18, 4).IsRequired();
            q.Property(x => x.UnitCode).HasColumnName("unit_code").HasMaxLength(10).IsRequired();
        });

        builder.OwnsOne(s => s.QuantityReserved, q =>
        {
            q.Property(x => x.Value).HasColumnName("qty_reserved").HasPrecision(18, 4).IsRequired();
            q.Property(x => x.UnitCode).HasColumnName("unit_code_reserved").HasMaxLength(10).IsRequired();
        });

        // One StockItem per Product per Warehouse per Tenant.
        builder.HasIndex(s => new { s.TenantId, s.ProductId, s.WarehouseId })
            .IsUnique()
            .HasDatabaseName("ix_stock_items_tenant_product_warehouse");

        builder.HasIndex(s => s.TenantId)
            .HasDatabaseName("ix_stock_items_tenant_id");

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}