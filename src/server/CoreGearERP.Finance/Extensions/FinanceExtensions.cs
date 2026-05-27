using CoreGearERP.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreGearERP.Finance.Extensions;

/// <summary>
/// DI service registrations for the Finance module.
/// </summary>
public static class FinanceExtensions
{
    /// <summary>
    /// Registers the FinanceDbContext with the dependency injection container, configuring it to use PostgreSQL with a connection string from the configuration. The DbContext is scoped to the finance schema in the database, ensuring that all data access for the Finance module is properly isolated. This method should be called in the application's startup configuration to ensure that the Finance module's data access layer is correctly set up and ready for use throughout the application.
    /// </summary>
    /// <param name="services">The IServiceCollection to which the FinanceDbContext will be added.</param>
    /// <param name="configuration">The IConfiguration from which the connection string for the database will be retrieved.</param>
    /// <returns>The updated IServiceCollection.</returns>
    public static IServiceCollection AddFinance(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<FinanceDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("CoreGearERP")!;
            options.UseNpgsql($"{connectionString};Search Path=finance");
        });

        return services;
    }
}