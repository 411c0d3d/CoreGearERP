using CoreGearERP.Sales.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGearERP.Sales.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping configuration for Shipment.
/// </summary>
public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("shipments");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(s => s.SalesOrderId).HasColumnName("sales_order_id").IsRequired();
        builder.Property(s => s.ShipmentNumber).HasColumnName("shipment_number").HasMaxLength(50).IsRequired();
        builder.Property(s => s.Notes).HasColumnName("notes").HasMaxLength(1000);
        builder.Property(s => s.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
        builder.Property(s => s.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(s => s.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(s => s.CreatedBy).HasColumnName("created_by").IsRequired();
        builder.Property(s => s.ModifiedAt).HasColumnName("modified_at").IsRequired();
        builder.Property(s => s.ModifiedBy).HasColumnName("modified_by").IsRequired();
        builder.Property(s => s.ConfirmedAt).HasColumnName("confirmed_at");
        builder.Property(s => s.CompletedAt).HasColumnName("completed_at");
        builder.Property(s => s.CancelledAt).HasColumnName("cancelled_at");

        builder.HasMany(s => s.Lines)
            .WithOne()
            .HasForeignKey(l => l.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.TenantId, s.ShipmentNumber })
            .IsUnique()
            .HasDatabaseName("ix_shipments_tenant_shipment_number");

        builder.HasIndex(s => s.TenantId)
            .HasDatabaseName("ix_shipments_tenant_id");

        builder.HasIndex(s => s.SalesOrderId)
            .HasDatabaseName("ix_shipments_sales_order_id");

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}