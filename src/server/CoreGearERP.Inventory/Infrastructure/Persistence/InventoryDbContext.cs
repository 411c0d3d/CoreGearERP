using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Common.Domain.Entities;
using CoreGearERP.Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Inventory.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext scoped to the Inventory module. Owns the inventory schema only.
/// </summary>
public class InventoryDbContext : DbContext
{
    private readonly ICurrentTenant _currentTenant;

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<StockItem> StockItems => Set<StockItem>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    /// <summary>
    /// Initializes a new instance of the InventoryDbContext class with the specified options and tenant context.
    /// </summary>
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options, ICurrentTenant currentTenant)
        : base(options)
    {
        _currentTenant = currentTenant;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("inventory");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
    }
}