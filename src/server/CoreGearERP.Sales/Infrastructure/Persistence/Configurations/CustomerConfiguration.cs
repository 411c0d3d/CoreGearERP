using CoreGearERP.Sales.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGearERP.Sales.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping configuration for Customer.
/// </summary>
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(c => c.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(c => c.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(c => c.ContactEmail).HasColumnName("contact_email").HasMaxLength(200).IsRequired();
        builder.Property(c => c.ContactPhone).HasColumnName("contact_phone").HasMaxLength(50);
        builder.Property(c => c.Address).HasColumnName("address").HasMaxLength(500);
        builder.Property(c => c.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
        builder.Property(c => c.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(c => c.CreatedBy).HasColumnName("created_by").IsRequired();
        builder.Property(c => c.ModifiedAt).HasColumnName("modified_at").IsRequired();
        builder.Property(c => c.ModifiedBy).HasColumnName("modified_by").IsRequired();
        builder.Property(c => c.ConfirmedAt).HasColumnName("confirmed_at");
        builder.Property(c => c.CompletedAt).HasColumnName("completed_at");
        builder.Property(c => c.CancelledAt).HasColumnName("cancelled_at");

        builder.HasIndex(c => new { c.TenantId, c.Code })
            .IsUnique()
            .HasDatabaseName("ix_customers_tenant_code");

        builder.HasIndex(c => c.TenantId)
            .HasDatabaseName("ix_customers_tenant_id");

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}