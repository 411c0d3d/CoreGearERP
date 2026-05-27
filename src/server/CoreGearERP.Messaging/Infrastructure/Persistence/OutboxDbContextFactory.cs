using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace CoreGearERP.Messaging.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core CLI tools.
/// Only used by dotnet ef migrations -- never at runtime.
/// </summary>
public class OutboxDbContextFactory : IDesignTimeDbContextFactory<OutboxDbContext>
{
    private const string Schema = "messaging";

    /// <summary>
    /// Creates a new instance of the OutboxDbContext class with the specified options.
    /// </summary>
    /// <param name="args">The command-line arguments passed to the design-time factory, typically ignored in this implementation.</param>
    public OutboxDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../CoreGearERP.Host"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var baseConnection = configuration.GetConnectionString("CoreGearERP")
                             ?? throw new InvalidOperationException("CoreGearERP connection string is not configured.");

        EnsureSchemaExists(baseConnection);

        var options = new DbContextOptionsBuilder<OutboxDbContext>()
            .UseNpgsql($"{baseConnection};Search Path={Schema}")
            .Options;

        return new OutboxDbContext(options);
    }

    private static void EnsureSchemaExists(string connectionString)
    {
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"CREATE SCHEMA IF NOT EXISTS {Schema};";
        command.ExecuteNonQuery();
    }
}