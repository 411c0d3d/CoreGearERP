using CoreGearERP.Production.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGearERP.Production.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping configuration for BillOfMaterials.
/// </summary>
public class BillOfMaterialsConfiguration : IEntityTypeConfiguration<BillOfMaterials>
{
    public void Configure(EntityTypeBuilder<BillOfMaterials> builder)
    {
        builder.ToTable("bills_of_materials");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id).HasColumnName("id");
        builder.Property(b => b.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(b => b.FinishedProductId).HasColumnName("finished_product_id").IsRequired();
        builder.Property(b => b.FinishedProductCode).HasColumnName("finished_product_code").HasMaxLength(50).IsRequired();
        builder.Property(b => b.FinishedProductName).HasColumnName("finished_product_name").HasMaxLength(200).IsRequired();
        builder.Property(b => b.Version).HasColumnName("version").HasMaxLength(50).IsRequired();
        builder.Property(b => b.Notes).HasColumnName("notes").HasMaxLength(1000);
        builder.Property(b => b.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
        builder.Property(b => b.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(b => b.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(b => b.CreatedBy).HasColumnName("created_by").IsRequired();
        builder.Property(b => b.ModifiedAt).HasColumnName("modified_at").IsRequired();
        builder.Property(b => b.ModifiedBy).HasColumnName("modified_by").IsRequired();
        builder.Property(b => b.ConfirmedAt).HasColumnName("confirmed_at");
        builder.Property(b => b.CompletedAt).HasColumnName("completed_at");
        builder.Property(b => b.CancelledAt).HasColumnName("cancelled_at");

        builder.HasMany(b => b.Lines)
            .WithOne()
            .HasForeignKey(l => l.BillOfMaterialsId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(b => new { b.TenantId, b.FinishedProductId, b.Version })
            .IsUnique()
            .HasDatabaseName("ix_bills_of_materials_tenant_product_version");

        builder.HasIndex(b => b.TenantId)
            .HasDatabaseName("ix_bills_of_materials_tenant_id");

        builder.HasQueryFilter(b => !b.IsDeleted);
    }
}