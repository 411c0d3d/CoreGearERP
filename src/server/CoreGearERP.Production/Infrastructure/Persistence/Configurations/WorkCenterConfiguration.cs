using CoreGearERP.Production.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGearERP.Production.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping configuration for WorkCenter.
/// </summary>
public class WorkCenterConfiguration : IEntityTypeConfiguration<WorkCenter>
{
    public void Configure(EntityTypeBuilder<WorkCenter> builder)
    {
        builder.ToTable("work_centers");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id).HasColumnName("id");
        builder.Property(w => w.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(w => w.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(w => w.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(w => w.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(w => w.CapacityPerHour).HasColumnName("capacity_per_hour").HasPrecision(18, 4).IsRequired();
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
            .HasDatabaseName("ix_work_centers_tenant_code");

        builder.HasIndex(w => w.TenantId)
            .HasDatabaseName("ix_work_centers_tenant_id");

        builder.HasQueryFilter(w => !w.IsDeleted);
    }
}