using Xunit;
using CoreGearERP.Tests.Infrastructure.Fixtures;
using CoreGearERP.Tests.Infrastructure.Helpers;
using Xunit.Abstractions;

namespace CoreGearERP.Tests.Infrastructure;

/// <summary>
/// Base class for all integration tests. Provides a pre-authenticated HttpClient,
/// runs EF migrations on first use, and resets data via /test/reset before each test class.
/// </summary>
[Collection(IntegrationTestCollection.Names.Integration)]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly IntegrationTestWebFactory _factory;

    /// <summary>
    /// Gets the authenticated HTTP client for making requests against the test host.
    /// </summary>
    protected HttpClient Client { get; private set; } = null!;

    /// <summary>
    /// Gets the seed helper for creating test data via the HTTP API.
    /// </summary>
    protected SeedHelper Seed { get; private set; } = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegrationTestBase"/> class.
    /// </summary>
    /// <param name="fixture">The shared integration test fixture providing container instances.</param>
    protected IntegrationTestBase(IntegrationTestFixture fixture)
    {
        _factory = new IntegrationTestWebFactory(fixture);
    }

    /// <summary>
    /// Runs migrations and resets the database to a clean state before each test class.
    /// </summary>
    public virtual async Task InitializeAsync()
    {
        await _factory.MigrateAsync();

        Client = _factory.CreateClient();
        Client.DefaultRequestHeaders.Add("Authorization", AuthHelper.BearerHeaderValue());
        Seed = new SeedHelper(Client);

        var response = await Client.DeleteAsync("/test/reset");
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Disposes the HTTP client and the web application factory.
    /// </summary>
    public virtual async Task DisposeAsync()
    {
        Client.Dispose();
        await _factory.DisposeAsync();
    }
}