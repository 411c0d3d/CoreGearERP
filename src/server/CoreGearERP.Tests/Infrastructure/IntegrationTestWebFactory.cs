using System.Text;
using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Finance.Infrastructure.Persistence;
using CoreGearERP.Inventory.Application.Contracts;
using CoreGearERP.Inventory.Infrastructure.Persistence;
using CoreGearERP.Messaging.Infrastructure.Persistence;
using CoreGearERP.Procurement.Infrastructure.Persistence;
using CoreGearERP.Production.Infrastructure.Persistence;
using CoreGearERP.Sales.Infrastructure.Persistence;
using CoreGearERP.Tests.Infrastructure.Fixtures;
using CoreGearERP.Tests.Infrastructure.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

namespace CoreGearERP.Tests.Infrastructure;

/// <summary>
/// Configures the Host under test with Testcontainer connection strings,
/// a deterministic JWT signing key, and Development environment so /test/reset is available.
/// </summary>
public sealed class IntegrationTestWebFactory : WebApplicationFactory<Program>
{
    private readonly IntegrationTestFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegrationTestWebFactory"/> class.
    /// </summary>
    public IntegrationTestWebFactory(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    /// <inheritdoc/>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:CoreGearERP"] = _fixture.Postgres.ConnectionString,
                ["Auth:SecretKey"] = AuthHelper.TestSigningKey,
                ["Auth:Issuer"] = AuthHelper.Issuer,
                ["Auth:Audience"] = AuthHelper.Audience,
                ["RabbitMq:Host"] = _fixture.RabbitMq.Host,
                ["RabbitMq:Port"] = _fixture.RabbitMq.Port.ToString(),
                ["RabbitMq:Username"] = _fixture.RabbitMq.Username,
                ["RabbitMq:Password"] = _fixture.RabbitMq.Password,
                ["MassTransit:OutboxQueryDelay"] = "00:00:01",
            });
        });

        builder.ConfigureServices((_, services) =>
        {
            ReplaceService<IInventoryCommandService, InventoryCommandService>(services);
            ReplaceService<IInventoryQueryService, InventoryQueryService>(services);

            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = AuthHelper.Issuer,
                    ValidateAudience = true,
                    ValidAudience = AuthHelper.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(AuthHelper.TestSigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5),
                };
            });
        });
    }

    /// <summary>
    /// Creates all required PostgreSQL schemas then runs EF Core migrations for every module DbContext.
    /// Builds DbContexts directly from the fixture connection string to avoid dependency on the DI service provider.
    /// </summary>
    public async Task MigrateAsync()
    {
        var connectionString = _fixture.Postgres.ConnectionString;

        await CreateSchemasAsync(connectionString);

        await MigrateDbContextAsync<InventoryDbContext>(connectionString);
        await MigrateDbContextAsync<ProcurementDbContext>(connectionString);
        await MigrateDbContextAsync<ProductionDbContext>(connectionString);
        await MigrateDbContextAsync<SalesDbContext>(connectionString);
        await MigrateDbContextAsync<FinanceDbContext>(connectionString);
        await MigrateDbContextAsync<OutboxDbContext>(connectionString);

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'messaging'";
        var count = (long)(await cmd.ExecuteScalarAsync())!;
        if (count == 0)
        {
            throw new InvalidOperationException("Outbox migration failed -- no tables found in messaging schema.");
        }
    }

    /// <summary>
    /// Runs EF Core migrations for a single DbContext using a direct connection string.
    /// </summary>
    private static async Task MigrateDbContextAsync<TContext>(string connectionString)
        where TContext : DbContext
    {
        var options = new DbContextOptionsBuilder<TContext>()
            .UseNpgsql(connectionString)
            .Options;

        await using var context = (TContext)Activator.CreateInstance(typeof(TContext), options)!;
        await context.Database.MigrateAsync();
    }

    /// <summary>
    /// Creates the module-level PostgreSQL schemas if they do not already exist.
    /// </summary>
    private static async Task CreateSchemasAsync(string connectionString)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
                          CREATE SCHEMA IF NOT EXISTS inventory;
                          CREATE SCHEMA IF NOT EXISTS procurement;
                          CREATE SCHEMA IF NOT EXISTS production;
                          CREATE SCHEMA IF NOT EXISTS sales;
                          CREATE SCHEMA IF NOT EXISTS finance;
                          CREATE SCHEMA IF NOT EXISTS messaging;
                          """;

        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Removes any existing registration for <typeparamref name="TService"/> and registers <typeparamref name="TImplementation"/> as scoped.
    /// </summary>
    private static void ReplaceService<TService, TImplementation>(IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        var existing = services.Where(d => d.ServiceType == typeof(TService)).ToList();
        foreach (var descriptor in existing)
        {
            services.Remove(descriptor);
        }

        services.AddScoped<TService, TImplementation>();
    }
}