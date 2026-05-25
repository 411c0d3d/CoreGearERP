using CoreGearERP.Sales.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGearERP.Sales.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping configuration for ShipmentLine.
/// </summary>
public class ShipmentLineConfiguration : IEntityTypeConfiguration<ShipmentLine>
{
    public void Configure(EntityTypeBuilder<ShipmentLine> builder)
    {
        builder.ToTable("shipment_lines");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id).HasColumnName("id");
        builder.Property(l => l.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(l => l.ShipmentId).HasColumnName("shipment_id").IsRequired();
        builder.Property(l => l.SalesOrderLineId).HasColumnName("sales_order_line_id").IsRequired();
        builder.Property(l => l.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(l => l.ProductCode).HasColumnName("product_code").HasMaxLength(50).IsRequired();
        builder.Property(l => l.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
        builder.Property(l => l.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(l => l.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(l => l.CreatedBy).HasColumnName("created_by").IsRequired();
        builder.Property(l => l.ModifiedAt).HasColumnName("modified_at").IsRequired();
        builder.Property(l => l.ModifiedBy).HasColumnName("modified_by").IsRequired();
        builder.Property(l => l.ConfirmedAt).HasColumnName("confirmed_at");
        builder.Property(l => l.CompletedAt).HasColumnName("completed_at");
        builder.Property(l => l.CancelledAt).HasColumnName("cancelled_at");

        builder.OwnsOne(l => l.QuantityShipped, q =>
        {
            q.Property(x => x.Value).HasColumnName("qty_shipped").HasPrecision(18, 4).IsRequired();
            q.Property(x => x.UnitCode).HasColumnName("unit_code").HasMaxLength(10).IsRequired();
        });

        builder.HasIndex(l => l.ShipmentId)
            .HasDatabaseName("ix_shipment_lines_shipment_id");

        builder.HasIndex(l => l.TenantId)
            .HasDatabaseName("ix_shipment_lines_tenant_id");

        builder.HasQueryFilter(l => !l.IsDeleted);
    }
}