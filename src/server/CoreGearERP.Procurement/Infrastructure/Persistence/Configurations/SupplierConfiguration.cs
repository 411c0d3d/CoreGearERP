using CoreGearERP.Procurement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGearERP.Procurement.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping configuration for the Supplier entity.
/// </summary>
public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("suppliers");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(s => s.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(s => s.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(s => s.ContactEmail).HasColumnName("contact_email").HasMaxLength(200).IsRequired();
        builder.Property(s => s.ContactPhone).HasColumnName("contact_phone").HasMaxLength(50);
        builder.Property(s => s.Address).HasColumnName("address").HasMaxLength(500);
        builder.Property(s => s.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
        builder.Property(s => s.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(s => s.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(s => s.CreatedBy).HasColumnName("created_by").IsRequired();
        builder.Property(s => s.ModifiedAt).HasColumnName("modified_at").IsRequired();
        builder.Property(s => s.ModifiedBy).HasColumnName("modified_by").IsRequired();
        builder.Property(s => s.ConfirmedAt).HasColumnName("confirmed_at");
        builder.Property(s => s.CompletedAt).HasColumnName("completed_at");
        builder.Property(s => s.CancelledAt).HasColumnName("cancelled_at");

        builder.HasIndex(s => new { s.TenantId, s.Code })
            .IsUnique()
            .HasDatabaseName("ix_suppliers_tenant_code");

        builder.HasIndex(s => s.TenantId)
            .HasDatabaseName("ix_suppliers_tenant_id");

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}