using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Inventory.Infrastructure.Persistence;

/// <summary>
/// EF Core ModelBuilder extensions for the Inventory module.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Applies a global query filter to all entities assignable to T.
    /// </summary>
    public static void ApplyGlobalFilters<T>(
        this ModelBuilder modelBuilder,
        Expression<Func<T, bool>> expression) where T : class
    {
        var entities = modelBuilder.Model
            .GetEntityTypes()
            .Where(e => e.ClrType.IsAssignableTo(typeof(T)));

        foreach (var entity in entities)
        {
            modelBuilder.Entity(entity.ClrType).HasQueryFilter(expression);
        }
    }
}