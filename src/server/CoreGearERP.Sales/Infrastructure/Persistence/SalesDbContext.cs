using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Sales.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Sales.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext scoped to the Sales module. Owns the Sales schema only.
/// </summary>
public class SalesDbContext : DbContext
{
    private readonly ICurrentTenant _currentTenant;

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();

    public DbSet<Shipment> Shipments => Set<Shipment>();

    // used for direct line queries in reporting -- accessed via Include() in current handlers
    public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();

    // used for direct line queries in reporting -- accessed via Include() in current handlers
    public DbSet<ShipmentLine> ShipmentLines => Set<ShipmentLine>();

    /// <summary>
    /// Creates a new instance of the SalesDbContext.
    /// </summary>
    /// <param name="options">The options for configuring the DbContext.</param>
    /// <param name="currentTenant">The current tenant context for multi-tenancy support.</param>
    public SalesDbContext(DbContextOptions<SalesDbContext> options, ICurrentTenant currentTenant)
        : base(options)
    {
        _currentTenant = currentTenant;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("sales");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SalesDbContext).Assembly);
    }
}