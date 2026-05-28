using Testcontainers.RabbitMq;
using Xunit;

namespace CoreGearERP.Tests.Infrastructure.Fixtures;

/// <summary>
/// Manages a shared RabbitMQ Testcontainer for the test collection lifetime.
/// </summary>
public sealed class RabbitMqFixture : IAsyncLifetime
{
    private readonly RabbitMqContainer _container = new RabbitMqBuilder("rabbitmq:3.13-management-alpine")
        .WithUsername("guest")
        .WithPassword("guest")
        .Build();

    /// <summary>
    /// Gets the hostname of the running RabbitMQ container.
    /// </summary>
    public string Host => _container.Hostname;

    /// <summary>
    /// Gets the mapped public port for AMQP connections.
    /// </summary>
    public ushort Port => _container.GetMappedPublicPort(5672);

    /// <summary>
    /// Gets the RabbitMQ username.
    /// </summary>
    public string Username => "guest";

    /// <summary>
    /// Gets the RabbitMQ password.
    /// </summary>
    public string Password => "guest";

    /// <summary>
    /// Starts the RabbitMQ container.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    /// <summary>
    /// Disposes the RabbitMQ container.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}