using CoreGearERP.Common.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace CoreGearERP.Inventory.Infrastructure.Persistence;

/// <summary>
/// Design time factory for EF Core CLI tools.
/// Only used by dotnet ef migrations -- never at runtime.
/// Reads connection string from appsettings to avoid hardcoded values.
/// </summary>
public class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    private const string Schema = "inventory";

    public InventoryDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../CoreGearERP.Host"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("Inventory")
            ?? throw new InvalidOperationException("Inventory connection string is not configured.");

        EnsureSchemaExists(connectionString);

        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new InventoryDbContext(options, new DesignTimeCurrentTenant());
    }

    private static void EnsureSchemaExists(string connectionString)
    {
        // Strip Search Path from connection string before opening a raw connection.
        // The schema may not exist yet so we cannot set it as the default path.
        var baseConnection = connectionString
            .Split(';')
            .Where(p => !p.Trim().StartsWith("Search Path", StringComparison.OrdinalIgnoreCase))
            .Aggregate((a, b) => $"{a};{b}");

        using var connection = new NpgsqlConnection(baseConnection);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"CREATE SCHEMA IF NOT EXISTS {Schema};";
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Stub tenant used only during design time. TenantId is irrelevant for migrations.
    /// </summary>
    private class DesignTimeCurrentTenant : ICurrentTenant
    {
        public Guid TenantId => Guid.Empty;
    }
}