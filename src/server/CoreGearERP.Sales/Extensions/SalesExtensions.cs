using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreGearERP.Sales.Extensions;

/// <summary>
/// Registers all Sales module services into the DI container.
/// </summary>
public static class SalesExtensions
{
    /// <summary>
    /// Registers the SalesDbContext with the connection string from configuration.
    /// </summary>
    /// <param name="services">The injected services</param>
    /// <param name="configuration">the application configuration.</param>
    /// <returns>The collection of registered services.</returns>
    public static IServiceCollection AddSalesModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<SalesDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("CoreGearERP") + ";Search Path=sales"));

        var assembly = typeof(SalesExtensions).Assembly;

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