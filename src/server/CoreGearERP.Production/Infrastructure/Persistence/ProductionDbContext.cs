using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Production.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Production.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext scoped to the Production module. Owns the production schema only.
/// </summary>
public class ProductionDbContext : DbContext
{
    private readonly ICurrentTenant _currentTenant;

    public DbSet<WorkCenter> WorkCenters => Set<WorkCenter>();

    public DbSet<BillOfMaterials> BillsOfMaterials => Set<BillOfMaterials>();

    public DbSet<BillOfMaterialsLine> BomLines => Set<BillOfMaterialsLine>();
    
    public DbSet<ProductionOrder> ProductionOrders => Set<ProductionOrder>();

    /// <summary>
    /// Creates a new instance of the ProductionDbContext.
    /// </summary>
    /// <param name="options">The options for configuring the DbContext.</param>
    /// <param name="currentTenant">The current tenant context for multi-tenancy support.</param>
    public ProductionDbContext(DbContextOptions<ProductionDbContext> options, ICurrentTenant currentTenant)
        : base(options)
    {
        _currentTenant = currentTenant;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("production");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductionDbContext).Assembly);
    }
}