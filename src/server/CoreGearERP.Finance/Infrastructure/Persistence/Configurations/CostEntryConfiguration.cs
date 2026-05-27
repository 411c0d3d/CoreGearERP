using CoreGearERP.Finance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGearERP.Finance.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping configuration for CostEntry.
/// </summary>
public class CostEntryConfiguration : IEntityTypeConfiguration<CostEntry>
{
    public void Configure(EntityTypeBuilder<CostEntry> builder)
    {
        builder.ToTable("CostEntries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("Id");
        builder.Property(e => e.TenantId).HasColumnName("TenantId").IsRequired();
        builder.Property(e => e.PeriodId).HasColumnName("PeriodId").IsRequired();
        builder.Property(e => e.SourceDocumentId).HasColumnName("SourceDocumentId").IsRequired();
        builder.Property(e => e.SourceDocumentNumber).HasColumnName("SourceDocumentNumber").HasMaxLength(50).IsRequired();
        builder.Property(e => e.SourceType).HasColumnName("SourceType").IsRequired();
        builder.Property(e => e.IsReversal).HasColumnName("IsReversal").IsRequired();
        builder.Property(e => e.ReversedCostEntryId).HasColumnName("ReversedCostEntryId");
        builder.Property(e => e.IsPendingCosting).HasColumnName("IsPendingCosting").IsRequired();
        builder.Property(e => e.Status).HasColumnName("Status").HasMaxLength(50).IsRequired();
        builder.Property(e => e.IsDeleted).HasColumnName("IsDeleted").IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("CreatedAt").IsRequired();
        builder.Property(e => e.CreatedBy).HasColumnName("CreatedBy").IsRequired();
        builder.Property(e => e.ModifiedAt).HasColumnName("ModifiedAt").IsRequired();
        builder.Property(e => e.ModifiedBy).HasColumnName("ModifiedBy").IsRequired();
        builder.Property(e => e.ConfirmedAt).HasColumnName("ConfirmedAt");
        builder.Property(e => e.CompletedAt).HasColumnName("CompletedAt");
        builder.Property(e => e.CancelledAt).HasColumnName("CancelledAt");

        builder.OwnsOne(e => e.Amount, m =>
        {
            m.Property(x => x.Amount).HasColumnName("Amount").HasPrecision(18, 4).IsRequired();
            m.Property(x => x.CurrencyCode).HasColumnName("CurrencyCode").HasMaxLength(3).IsRequired();
        });

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}