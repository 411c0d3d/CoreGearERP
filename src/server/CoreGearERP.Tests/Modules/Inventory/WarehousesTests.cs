using System.Net;
using System.Net.Http.Json;
using CoreGearERP.Tests.Infrastructure;
using CoreGearERP.Tests.Infrastructure.Fixtures;
using FluentAssertions;
using Xunit;

namespace CoreGearERP.Tests.Modules.Inventory;

/// <summary>
/// Integration tests for the /inventory/warehouses endpoints.
/// </summary>
public sealed class WarehousesTests : IntegrationTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WarehousesTests"/> class.
    /// </summary>
    /// <param name="fixture">The shared integration test fixture.</param>
    public WarehousesTests(IntegrationTestFixture fixture) 
        : base(fixture) { }

    [Fact]
    public async Task CreateWarehouse_ValidCommand_Returns201WithId()
    {
        var response = await Client.PostAsJsonAsync("/inventory/warehouses", new
        {
            code = "WH-001",
            name = "Main Warehouse",
            location = "Berlin, Germany",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<IdResponse>();
        body!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateWarehouse_DuplicateCode_Returns400()
    {
        await Seed.CreateWarehouseAsync("WH-DUP", "Main Warehouse", "Berlin, Germany");

        var response = await Client.PostAsJsonAsync("/inventory/warehouses", new
        {
            code = "WH-DUP",
            name = "Another Warehouse",
            location = "Hamburg, Germany",
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetWarehouses_AfterCreation_ReturnsCreatedWarehouse()
    {
        await Seed.CreateWarehouseAsync("WH-GET", "Get Warehouse", "Munich, Germany");

        var response = await Client.GetAsync("/inventory/warehouses");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<List<WarehouseResponse>>();
        body.Should().ContainSingle(w => w.Code == "WH-GET");
    }

    private sealed record IdResponse(Guid Id);

    private sealed record WarehouseResponse(Guid Id, string Code, string Name, string Location);
}