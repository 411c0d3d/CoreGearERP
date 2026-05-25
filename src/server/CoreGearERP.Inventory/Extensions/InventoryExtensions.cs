using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Inventory.Application.Contracts;
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
    public static IServiceCollection AddInventoryModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<InventoryDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("CoreGearERP") + ";Search Path=inventory"));

        services.AddScoped<IInventoryCommandService, InventoryCommandService>();

        // Register all command and query handlers in this module.
        var assembly = typeof(InventoryExtensions).Assembly;

        foreach (var type in assembly.GetTypes().Where(t => !t.IsAbstract && !t.IsInterface))
        {
            foreach (var iface in type.GetInterfaces())
            {
                if (!iface.IsGenericType)
                {
                    continue;
                }

                var definition = iface.GetGenericTypeDefinition();

                if (definition == typeof(ICommandHandler<,>) || definition == typeof(IQueryHandler<,>))
                {
                    services.AddScoped(iface, type);
                }
            }
        }

        return services;
    }
}