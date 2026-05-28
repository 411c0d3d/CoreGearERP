using System.Net;
using System.Net.Http.Json;
using CoreGearERP.Tests.Infrastructure;
using CoreGearERP.Tests.Infrastructure.Fixtures;
using FluentAssertions;
using Xunit;

namespace CoreGearERP.Tests.Modules.Inventory;

/// <summary>
/// Integration tests for the /inventory/stock-items endpoints.
/// </summary>
public sealed class StockItemsTests : IntegrationTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StockItemsTests"/> class.
    /// </summary>
    /// <param name="fixture">The shared integration test fixture.</param>
    public StockItemsTests(IntegrationTestFixture fixture) 
        : base(fixture) { }

    [Fact]
    public async Task CreateStockItem_ValidCommand_Returns201WithId()
    {
        var productId = await Seed.CreateProductAsync("PROD-SI-001", "Steel Rod", "KG");
        var warehouseId = await Seed.CreateWarehouseAsync("WH-SI-001", "Main Warehouse", "Berlin, Germany");

        var response = await Client.PostAsJsonAsync("/inventory/stock-items", new
        {
            productId,
            warehouseId,
            unitCode = "KG",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<IdResponse>();
        body!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateStockItem_DuplicateProductWarehouse_Returns400()
    {
        var productId = await Seed.CreateProductAsync("PROD-SI-DUP", "Steel Rod", "KG");
        var warehouseId = await Seed.CreateWarehouseAsync("WH-SI-DUP", "Main Warehouse", "Berlin, Germany");

        await Seed.CreateStockItemAsync(productId, warehouseId, "KG");

        var response = await Client.PostAsJsonAsync("/inventory/stock-items", new
        {
            productId,
            warehouseId,
            unitCode = "KG",
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetStockItems_FilterByWarehouse_ReturnsOnlyMatchingItems()
    {
        var productId = await Seed.CreateProductAsync("PROD-SI-FILT", "Steel Rod", "KG");
        var warehouseId = await Seed.CreateWarehouseAsync("WH-SI-FILT", "Filter Warehouse", "Berlin, Germany");
        var otherWarehouseId = await Seed.CreateWarehouseAsync("WH-SI-OTHER", "Other Warehouse", "Hamburg, Germany");

        await Seed.CreateStockItemAsync(productId, warehouseId, "KG");

        var productId2 = await Seed.CreateProductAsync("PROD-SI-FILT2", "Aluminium Sheet", "PCS");
        await Seed.CreateStockItemAsync(productId2, otherWarehouseId, "PCS");

        var response = await Client.GetAsync($"/inventory/stock-items?warehouseId={warehouseId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<List<StockItemResponse>>();
        body.Should().NotBeNull();
        body.Should().Contain(s => s.WarehouseId == warehouseId);
        body.Should().NotContain(s => s.WarehouseId == otherWarehouseId);
    }

    [Fact]
    public async Task GetStockMovements_NoMovements_ReturnsEmptyList()
    {
        var productId = await Seed.CreateProductAsync("PROD-MOV-001", "Steel Rod", "KG");
        var warehouseId = await Seed.CreateWarehouseAsync("WH-MOV-001", "Main Warehouse", "Berlin, Germany");
        var stockItemId = await Seed.CreateStockItemAsync(productId, warehouseId, "KG");

        var response = await Client.GetAsync($"/inventory/stock-items/{stockItemId}/movements");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<List<object>>();
        body.Should().BeEmpty();
    }

    private sealed record IdResponse(Guid Id);

    private sealed record StockItemResponse(Guid Id, Guid ProductId, Guid WarehouseId, string UnitCode);
}