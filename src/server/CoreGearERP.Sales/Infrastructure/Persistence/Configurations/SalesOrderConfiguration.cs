using CoreGearERP.Sales.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGearERP.Sales.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping configuration for SalesOrder.
/// </summary>
public class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.ToTable("sales_orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id).HasColumnName("id");
        builder.Property(o => o.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(o => o.OrderNumber).HasColumnName("order_number").HasMaxLength(50).IsRequired();
        builder.Property(o => o.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(o => o.CustomerName).HasColumnName("customer_name").HasMaxLength(200).IsRequired();
        builder.Property(o => o.Notes).HasColumnName("notes").HasMaxLength(1000);
        builder.Property(o => o.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
        builder.Property(o => o.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(o => o.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(o => o.CreatedBy).HasColumnName("created_by").IsRequired();
        builder.Property(o => o.ModifiedAt).HasColumnName("modified_at").IsRequired();
        builder.Property(o => o.ModifiedBy).HasColumnName("modified_by").IsRequired();
        builder.Property(o => o.ConfirmedAt).HasColumnName("confirmed_at");
        builder.Property(o => o.CompletedAt).HasColumnName("completed_at");
        builder.Property(o => o.CancelledAt).HasColumnName("cancelled_at");

        builder.HasMany(o => o.Lines)
            .WithOne()
            .HasForeignKey(l => l.SalesOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => new { o.TenantId, o.OrderNumber })
            .IsUnique()
            .HasDatabaseName("ix_sales_orders_tenant_order_number");

        builder.HasIndex(o => o.TenantId)
            .HasDatabaseName("ix_sales_orders_tenant_id");

        builder.HasIndex(o => o.CustomerId)
            .HasDatabaseName("ix_sales_orders_customer_id");

        builder.HasQueryFilter(o => !o.IsDeleted);
    }
}