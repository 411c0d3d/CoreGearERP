using Xunit;
using CoreGearERP.Tests.Infrastructure.Fixtures;
using CoreGearERP.Tests.Infrastructure.Helpers;

namespace CoreGearERP.Tests.Infrastructure;

/// <summary>
/// Base class for all integration tests. Provides a pre-authenticated HttpClient
/// and resets data via /test/reset before each test class.
/// </summary>
[Collection(IntegrationTestCollection.Names.Integration)]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;
    private IntegrationTestWebFactory _factory = null!;

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
        _fixture = fixture;
    }

    /// <summary>
    /// Creates the test host and resets the database to a clean state before each test class.
    /// </summary>
    public virtual async Task InitializeAsync()
    {
        // Factory is created here so containers are guaranteed started by this point.
        _factory = new IntegrationTestWebFactory(_fixture);

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