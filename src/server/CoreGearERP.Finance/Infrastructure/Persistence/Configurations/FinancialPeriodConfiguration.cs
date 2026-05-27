using CoreGearERP.Finance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGearERP.Finance.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for FinancialPeriod.
/// </summary>
public class FinancialPeriodConfiguration : IEntityTypeConfiguration<FinancialPeriod>
{
    public void Configure(EntityTypeBuilder<FinancialPeriod> builder)
    {
        builder.ToTable("FinancialPeriods");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(p => p.StartDate)
            .IsRequired();

        builder.Property(p => p.EndDate)
            .IsRequired();

        builder.Property(p => p.Status)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(p => p.TenantId)
            .IsRequired();

        builder.HasIndex(p => new { p.TenantId, p.Name }).IsUnique();
        builder.HasIndex(p => p.TenantId);

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}