using CoreGearERP.Inventory.Domain.Entities;
using CoreGearERP.Inventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGearERP.Inventory.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping configuration for the StockMovement entity.
/// </summary>
public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("stock_movements");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(m => m.StockItemId).HasColumnName("stock_item_id").IsRequired();
        builder.Property(m => m.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(m => m.WarehouseId).HasColumnName("warehouse_id").IsRequired();
        builder.Property(m => m.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
        builder.Property(m => m.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(m => m.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(m => m.CreatedBy).HasColumnName("created_by").IsRequired();
        builder.Property(m => m.ModifiedAt).HasColumnName("modified_at").IsRequired();
        builder.Property(m => m.ModifiedBy).HasColumnName("modified_by").IsRequired();
        builder.Property(m => m.ConfirmedAt).HasColumnName("confirmed_at");
        builder.Property(m => m.CompletedAt).HasColumnName("completed_at");
        builder.Property(m => m.CancelledAt).HasColumnName("cancelled_at");
        builder.Property(m => m.ReferenceId).HasColumnName("reference_id");
        builder.Property(m => m.ReferenceNumber).HasColumnName("reference_number").HasMaxLength(100);
        builder.Property(m => m.Notes).HasColumnName("notes").HasMaxLength(500);

        builder.Property(m => m.MovementType)
            .HasColumnName("movement_type")
            .HasMaxLength(50)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<StockMovementType>(v))
            .IsRequired();

        builder.OwnsOne(m => m.Quantity, q =>
        {
            q.Property(x => x.Value).HasColumnName("quantity").HasPrecision(18, 4).IsRequired();
            q.Property(x => x.UnitCode).HasColumnName("unit_code").HasMaxLength(10).IsRequired();
        });

        builder.HasIndex(m => m.TenantId).HasDatabaseName("ix_stock_movements_tenant_id");
        builder.HasIndex(m => m.StockItemId).HasDatabaseName("ix_stock_movements_stock_item_id");
        builder.HasIndex(m => m.ProductId).HasDatabaseName("ix_stock_movements_product_id");
        builder.HasIndex(m => m.CreatedAt).HasDatabaseName("ix_stock_movements_created_at");

        // StockMovements are immutable -- no soft delete filter needed.
        // They are never deleted, only compensated with counter-movements.
    }
}