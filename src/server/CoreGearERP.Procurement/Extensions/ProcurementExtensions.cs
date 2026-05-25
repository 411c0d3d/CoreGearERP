using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Procurement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreGearERP.Procurement.Extensions;

/// <summary>
/// Registers all Procurement module services into the DI container.
/// </summary>
public static class ProcurementExtensions
{
    
    /// <summary>
    /// Adds the Procurement module's DbContext and automatically registers all command and query handlers found in the assembly.
    /// </summary>
    /// <param name="services">The service collection to add the module's services to.</param>
    /// <param name="configuration">The application configuration, used to retrieve the database connection string for the Procurement module.</param>
    public static IServiceCollection AddProcurementModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ProcurementDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("CoreGearERP") + ";Search Path=procurement"));

        var assembly = typeof(ProcurementExtensions).Assembly;

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