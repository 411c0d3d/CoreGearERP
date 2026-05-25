using CoreGearERP.Common.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace CoreGearERP.Production.Infrastructure.Persistence;

/// <summary>
/// Design time factory for EF Core CLI tools.
/// Only used by dotnet ef migrations -- never at runtime.
/// </summary>
public class ProductionDbContextFactory : IDesignTimeDbContextFactory<ProductionDbContext>
{
    private const string Schema = "production";

    /// <summary>
    /// Creates a new instance of the ProductionDbContext for design-time operations.
    /// </summary>
    /// <param name="args">Command-line arguments (not used).</param>
    public ProductionDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../CoreGearERP.Host"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var baseConnection = configuration.GetConnectionString("CoreGearERP")
                             ?? throw new InvalidOperationException("CoreGearERP connection string is not configured.");

        EnsureSchemaExists(baseConnection);

        var options = new DbContextOptionsBuilder<ProductionDbContext>()
            .UseNpgsql($"{baseConnection};Search Path={Schema}")
            .Options;

        return new ProductionDbContext(options, new DesignTimeCurrentTenant());
    }

    private static void EnsureSchemaExists(string connectionString)
    {
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"CREATE SCHEMA IF NOT EXISTS {Schema};";
        command.ExecuteNonQuery();
    }

    private class DesignTimeCurrentTenant : ICurrentTenant
    {
        public Guid TenantId => Guid.Empty;
    }
}