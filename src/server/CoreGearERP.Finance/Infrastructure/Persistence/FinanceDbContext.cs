using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Common.Domain.ValueObjects;
using CoreGearERP.Finance.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Finance.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext scoped to the Finance module. Owns the finance schema only.
/// </summary>
public class FinanceDbContext : DbContext
{
    private readonly ICurrentTenant _currentTenant;

    public DbSet<FinancialPeriod> FinancialPeriods => Set<FinancialPeriod>();
    public DbSet<CostEntry> CostEntries => Set<CostEntry>();

    /// <summary>
    /// Initializes a new instance of the FinanceDbContext class with the specified options and tenant context.
    /// </summary>
    /// <param name="options">The options for configuring the DbContext, typically injected by the dependency injection container.</param>
    /// <param name="currentTenant">The ICurrentTenant service is injected to access the current tenant's context, ensuring that all database operations are performed within the correct tenant scope.</param>
    public FinanceDbContext(DbContextOptions<FinanceDbContext> options, ICurrentTenant currentTenant)
        : base(options)
    {
        _currentTenant = currentTenant;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Owned<Money>();
        modelBuilder.HasDefaultSchema("finance");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinanceDbContext).Assembly);
    }
}