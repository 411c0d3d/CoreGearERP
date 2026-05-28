using System.Net;
using System.Net.Http.Json;
using CoreGearERP.Tests.Infrastructure;
using CoreGearERP.Tests.Infrastructure.Fixtures;
using FluentAssertions;
using Xunit;

namespace CoreGearERP.Tests.Modules.Inventory;

/// <summary>
/// Integration tests for the /inventory/products endpoints.
/// </summary>
public sealed class ProductsTests : IntegrationTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProductsTests"/> class.
    /// </summary>
    /// <param name="fixture">The shared integration test fixture.</param>
    public ProductsTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task CreateProduct_ValidCommand_Returns201WithId()
    {
        var response = await Client.PostAsJsonAsync("/inventory/products", new
        {
            code = "PROD-001",
            name = "Steel Rod",
            unitCode = "KG",
            description = "Raw steel rod",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<IdResponse>();
        body!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateProduct_DuplicateCode_Returns400()
    {
        await Seed.CreateProductAsync("PROD-DUP", "Steel Rod", "KG");

        var response = await Client.PostAsJsonAsync("/inventory/products", new
        {
            code = "PROD-DUP",
            name = "Steel Rod",
            unitCode = "KG",
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetProducts_AfterCreation_ReturnsCreatedProduct()
    {
        await Seed.CreateProductAsync("PROD-GET", "Aluminium Sheet", "PCS");

        var response = await Client.GetAsync("/inventory/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<List<ProductResponse>>();
        body.Should().ContainSingle(p => p.Code == "PROD-GET");
    }

    [Fact]
    public async Task GetProducts_EmptyDatabase_ReturnsEmptyList()
    {
        var response = await Client.GetAsync("/inventory/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<List<ProductResponse>>();
        body.Should().BeEmpty();
    }

    private sealed record IdResponse(Guid Id);

    private sealed record ProductResponse(Guid Id, string Code, string Name, string UnitCode);
}