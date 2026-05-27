using CoreGearERP.Messaging.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreGearERP.Messaging.Extensions;

/// <summary>
/// Registers OutboxDbContext and MassTransit with RabbitMQ transport and single bus outbox.
/// </summary>
public static class MessagingExtensions
{
    /// <summary>
    /// Configures OutboxDbContext, MassTransit bus outbox, Finance consumers, and RabbitMQ transport.
    /// </summary>
    /// <param name="services">The service collection to add the messaging services to.</param>
    /// <param name="configuration">The application configuration to retrieve connection strings and other settings.</param>
    /// <returns>The updated service collection with messaging services registered.</returns>
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CoreGearERP")
                               ?? throw new InvalidOperationException("CoreGearERP connection string is not configured.");
 
        services.AddDbContext<OutboxDbContext>(options =>
            options.UseNpgsql($"{connectionString};Search Path=messaging"));
 
        return services;
    }

}