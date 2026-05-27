using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Common.Domain.ValueObjects;
using CoreGearERP.Procurement.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Procurement.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext scoped to the Procurement module. Owns the procurement schema only.
/// </summary>
public class ProcurementDbContext : DbContext
{
    private readonly ICurrentTenant _currentTenant;

    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
    public DbSet<GoodsReceipt> GoodsReceipts => Set<GoodsReceipt>();

    /// <summary>
    /// Initializes a new instance of the ProcurementDbContext class with the specified options and tenant context.
    /// </summary>
    /// <param name="options">The options for configuring the DbContext, typically injected by the dependency injection container.</param>
    /// <param name="currentTenant">The ICurrentTenant service is injected to access the current tenant's context, ensuring that all database operations are performed within the correct tenant scope.</param>
    public ProcurementDbContext(DbContextOptions<ProcurementDbContext> options, ICurrentTenant currentTenant)
        : base(options)
    {
        _currentTenant = currentTenant;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Owned<Quantity>();
        modelBuilder.Owned<Money>();

        modelBuilder.HasDefaultSchema("procurement");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProcurementDbContext).Assembly);
    }
}