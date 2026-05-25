using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Production.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreGearERP.Production.Extensions;

/// <summary>
/// Registers all Production module services into the DI container.
/// </summary>
public static class ProductionExtensions
{
    /// <summary>
    /// Adds the Production module's DbContext and all command/query handlers to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="configuration">The application configuration for retrieving connection strings.</param>
    public static IServiceCollection AddProductionModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ProductionDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("CoreGearERP") + ";Search Path=production"));

        var assembly = typeof(ProductionExtensions).Assembly;

        foreach (var type in assembly.GetTypes().Where(t => !t.IsAbstract && !t.IsInterface))
        {
            foreach (var iface in type.GetInterfaces())
            {
                if (!iface.IsGenericType) { continue; }

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