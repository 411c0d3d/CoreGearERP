using CoreGearERP.Production.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGearERP.Production.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping configuration for ProductionOrder.
/// </summary>
public class ProductionOrderConfiguration : IEntityTypeConfiguration<ProductionOrder>
{
    public void Configure(EntityTypeBuilder<ProductionOrder> builder)
    {
        builder.ToTable("production_orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id).HasColumnName("id");
        builder.Property(o => o.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(o => o.OrderNumber).HasColumnName("order_number").HasMaxLength(50).IsRequired();
        builder.Property(o => o.BillOfMaterialsId).HasColumnName("bill_of_materials_id").IsRequired();
        builder.Property(o => o.FinishedProductCode).HasColumnName("finished_product_code").HasMaxLength(50).IsRequired();
        builder.Property(o => o.FinishedProductName).HasColumnName("finished_product_name").HasMaxLength(200).IsRequired();
        builder.Property(o => o.WorkCenterId).HasColumnName("work_center_id").IsRequired();
        builder.Property(o => o.WorkCenterCode).HasColumnName("work_center_code").HasMaxLength(50).IsRequired();
        builder.Property(o => o.Notes).HasColumnName("notes").HasMaxLength(1000);
        builder.Property(o => o.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
        builder.Property(o => o.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(o => o.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(o => o.CreatedBy).HasColumnName("created_by").IsRequired();
        builder.Property(o => o.ModifiedAt).HasColumnName("modified_at").IsRequired();
        builder.Property(o => o.ModifiedBy).HasColumnName("modified_by").IsRequired();
        builder.Property(o => o.ConfirmedAt).HasColumnName("confirmed_at");
        builder.Property(o => o.CompletedAt).HasColumnName("completed_at");
        builder.Property(o => o.CancelledAt).HasColumnName("cancelled_at");

        builder.OwnsOne(o => o.PlannedQuantity, q =>
        {
            q.Property(x => x.Value).HasColumnName("planned_qty").HasPrecision(18, 4).IsRequired();
            q.Property(x => x.UnitCode).HasColumnName("planned_unit_code").HasMaxLength(10).IsRequired();
        });

        builder.OwnsOne(o => o.ActualQuantity, q =>
        {
            q.Property(x => x.Value).HasColumnName("actual_qty").HasPrecision(18, 4);
            q.Property(x => x.UnitCode).HasColumnName("actual_unit_code").HasMaxLength(10);
        });

        builder.HasIndex(o => new { o.TenantId, o.OrderNumber })
            .IsUnique()
            .HasDatabaseName("ix_production_orders_tenant_order_number");

        builder.HasIndex(o => o.TenantId)
            .HasDatabaseName("ix_production_orders_tenant_id");

        builder.HasIndex(o => o.BillOfMaterialsId)
            .HasDatabaseName("ix_production_orders_bom_id");

        builder.HasQueryFilter(o => !o.IsDeleted);
    }
}