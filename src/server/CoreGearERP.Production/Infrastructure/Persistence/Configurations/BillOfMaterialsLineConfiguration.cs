using CoreGearERP.Production.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGearERP.Production.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping configuration for BillOfMaterialsLine.
/// </summary>
public class BillOfMaterialsLineConfiguration : IEntityTypeConfiguration<BillOfMaterialsLine>
{
    public void Configure(EntityTypeBuilder<BillOfMaterialsLine> builder)
    {
        builder.ToTable("bill_of_materials_lines");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id).HasColumnName("id");
        builder.Property(l => l.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(l => l.BillOfMaterialsId).HasColumnName("bill_of_materials_id").IsRequired();
        builder.Property(l => l.ComponentProductId).HasColumnName("component_product_id").IsRequired();
        builder.Property(l => l.ComponentProductCode).HasColumnName("component_product_code").HasMaxLength(50).IsRequired();
        builder.Property(l => l.ComponentProductName).HasColumnName("component_product_name").HasMaxLength(200).IsRequired();
        builder.Property(l => l.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
        builder.Property(l => l.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(l => l.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(l => l.CreatedBy).HasColumnName("created_by").IsRequired();
        builder.Property(l => l.ModifiedAt).HasColumnName("modified_at").IsRequired();
        builder.Property(l => l.ModifiedBy).HasColumnName("modified_by").IsRequired();
        builder.Property(l => l.ConfirmedAt).HasColumnName("confirmed_at");
        builder.Property(l => l.CompletedAt).HasColumnName("completed_at");
        builder.Property(l => l.CancelledAt).HasColumnName("cancelled_at");

        builder.OwnsOne(l => l.QuantityRequired, q =>
        {
            q.Property(x => x.Value).HasColumnName("qty_required").HasPrecision(18, 4).IsRequired();
            q.Property(x => x.UnitCode).HasColumnName("unit_code").HasMaxLength(10).IsRequired();
        });

        builder.HasIndex(l => l.BillOfMaterialsId)
            .HasDatabaseName("ix_bill_of_materials_lines_bom_id");

        builder.HasIndex(l => l.TenantId)
            .HasDatabaseName("ix_bill_of_materials_lines_tenant_id");

        builder.HasQueryFilter(l => !l.IsDeleted);
    }
}