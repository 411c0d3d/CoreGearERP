using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Messaging.Infrastructure.Persistence;

/// <summary>
/// EF Core context owning MassTransit outbox and inbox tables only.
/// No domain entities. No module bleed.
/// </summary>
public class OutboxDbContext : DbContext
{
    /// <summary>
    /// Creates a new instance of the OutboxDbContext class with the specified options.
    /// </summary>
    /// <param name="options">The options for configuring the DbContext, typically injected by the dependency injection container.</param>
    public OutboxDbContext(DbContextOptions<OutboxDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("messaging");

        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddInboxStateEntity();
    }
}