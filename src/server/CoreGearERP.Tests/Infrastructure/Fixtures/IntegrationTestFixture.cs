using CoreGearERP.Tests.Infrastructure;
using System.Net.Sockets;
using Xunit;

namespace CoreGearERP.Tests.Infrastructure.Fixtures;

/// <summary>
/// Composes Postgres and RabbitMQ fixtures into a single collection-scoped fixture.
/// Started once per xUnit collection; containers and migrations are shared across all test classes.
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

        await WaitForRabbitMqAsync();

        await using var factory = new IntegrationTestWebFactory(this);
        await factory.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await Postgres.DisposeAsync();
        await RabbitMq.DisposeAsync();
    }

    /// <summary>
    /// Polls the RabbitMQ AMQP port until a TCP connection succeeds or timeout elapses.
    /// </summary>
    private async Task WaitForRabbitMqAsync()
    {
        var deadline = DateTime.UtcNow.AddSeconds(30);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                using var tcp = new TcpClient();
                await tcp.ConnectAsync(RabbitMq.Host, RabbitMq.Port);
                return;
            }
            catch
            {
                await Task.Delay(500);
            }
        }

        throw new TimeoutException("RabbitMQ did not become ready within 30 seconds.");
    }
}