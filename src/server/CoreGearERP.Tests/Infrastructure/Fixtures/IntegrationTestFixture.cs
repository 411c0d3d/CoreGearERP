using Xunit;

namespace CoreGearERP.Tests.Infrastructure.Fixtures;

/// <summary>
/// Composes Postgres and RabbitMQ fixtures into a single collection-scoped fixture.
/// Started once per xUnit collection; containers are shared across all test classes.
/// </summary>
public sealed class IntegrationTestFixture : IAsyncLifetime
{
    public PostgresFixture Postgres { get; } = new();
    public RabbitMqFixture RabbitMq { get; } = new();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            Postgres.InitializeAsync(),
            RabbitMq.InitializeAsync());
    }

    public async Task DisposeAsync()
    {
        await Postgres.DisposeAsync();
        await RabbitMq.DisposeAsync();
    }
}