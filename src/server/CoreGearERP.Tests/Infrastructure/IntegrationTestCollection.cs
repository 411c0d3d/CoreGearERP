using CoreGearERP.Tests.Infrastructure.Fixtures;
using Xunit;

namespace CoreGearERP.Tests.Infrastructure;

/// <summary>
/// Declares the shared xUnit collection that owns the container lifetime.
/// </summary>
[CollectionDefinition(Names.Integration)]
public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
{
    /// <summary>
    /// Collection name constants to avoid magic strings across test classes.
    /// </summary>
    public static class Names
    {
        public const string Integration = "Integration";
    }
}