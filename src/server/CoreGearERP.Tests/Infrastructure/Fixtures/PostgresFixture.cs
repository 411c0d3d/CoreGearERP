using Testcontainers.PostgreSql;
using Xunit;

namespace CoreGearERP.Tests.Infrastructure.Fixtures;

/// <summary>
/// Manages a shared PostgreSQL Testcontainer for the test collection lifetime.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("coregear_test")
        .WithUsername("coregear")
        .WithPassword("coregear_test_pw")
        .Build();

    /// <summary>
    /// Gets the connection string for the running Postgres container.
    /// </summary>
    public string ConnectionString => _container.GetConnectionString();

    /// <summary>
    /// Starts the Postgres container.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    /// <summary>
    /// Disposes the Postgres container.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}