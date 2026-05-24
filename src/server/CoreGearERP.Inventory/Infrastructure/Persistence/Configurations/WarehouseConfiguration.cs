using CoreGearERP.Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGearERP.Inventory.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration for the Warehouse entity, defining the database schema and constraints for the warehouses table.
/// </summary>
public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("warehouses");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id).HasColumnName("id");
        builder.Property(w => w.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(w => w.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(w => w.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(w => w.Location).HasColumnName("location").HasMaxLength(500);
        builder.Property(w => w.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
        builder.Property(w => w.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(w => w.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(w => w.CreatedBy).HasColumnName("created_by").IsRequired();
        builder.Property(w => w.ModifiedAt).HasColumnName("modified_at").IsRequired();
        builder.Property(w => w.ModifiedBy).HasColumnName("modified_by").IsRequired();
        builder.Property(w => w.ConfirmedAt).HasColumnName("confirmed_at");
        builder.Property(w => w.CompletedAt).HasColumnName("completed_at");
        builder.Property(w => w.CancelledAt).HasColumnName("cancelled_at");

        builder.HasIndex(w => new { w.TenantId, w.Code })
            .IsUnique()
            .HasDatabaseName("ix_warehouses_tenant_code");

        builder.HasIndex(w => w.TenantId)
            .HasDatabaseName("ix_warehouses_tenant_id");

        builder.HasQueryFilter(w => !w.IsDeleted);
    }
}