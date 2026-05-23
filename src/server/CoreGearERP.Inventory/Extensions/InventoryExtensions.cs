using CoreGearERP.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreGearERP.Inventory.Extensions;

/// <summary>
/// Registers all Inventory module services into the DI container.
/// </summary>
public static class InventoryExtensions
{
    /// <summary>
    /// Registers the InventoryDbContext with the connection string from configuration.
    /// </summary>
    /// <param name="services">The injected services</param>
    /// <param name="configuration">the application configuration.</param>
    /// <returns>The collection of registered services.</returns>
    public static IServiceCollection AddInventoryModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<InventoryDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Inventory")));

        return services;
    }
}