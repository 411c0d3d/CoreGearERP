using System.Net.Http.Json;
using CoreGearERP.Tests.Infrastructure;
using CoreGearERP.Tests.Infrastructure.Fixtures;
using CoreGearERP.Tests.Infrastructure.Helpers;
using FluentAssertions;
using Xunit;

namespace CoreGearERP.Tests.Modules.Procurement;

/// <summary>
/// Integration tests for goods receipt -- verifies stock movements and Finance cost entry via outbox.
/// </summary>
public sealed class ReceiveGoodsTests : IntegrationTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReceiveGoodsTests"/> class.
    /// </summary>
    /// <param name="fixture">The shared integration test fixture.</param>
    public ReceiveGoodsTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task ReceiveGoods_FullQuantity_StockMovementCreatedAndPOStatusIsReceived()
    {
        var productId = await Seed.CreateProductAsync("COMP-RG-001", "Raw Steel", "KG");
        var warehouseId = await Seed.CreateWarehouseAsync("WH-RG-001", "Receiving Warehouse", "Berlin, Germany");
        var stockItemId = await Seed.CreateStockItemAsync(productId, warehouseId, "KG");
        var supplierId = await Seed.CreateSupplierAsync("SUP-RG-001", "Steel Supplier GmbH", "rg@steel.de");

        var poId = await Seed.CreatePurchaseOrderAsync(
            supplierId, productId, "COMP-RG-001", "Raw Steel",
            quantity: 100, unitCode: "KG", unitPrice: 2.50m);

        await Seed.ConfirmPurchaseOrderAsync(poId);

        var po = await (await Client.GetAsync($"/procurement/orders/{poId}"))
            .Content.ReadFromJsonAsync<PurchaseOrderDetailResponse>();
        var lineId = po!.Lines[0].Id;

        await Seed.ReceiveGoodsAsync(poId, lineId, warehouseId, quantity: 100);

        var movements = await (await Client.GetAsync($"/inventory/stock-items/{stockItemId}/movements"))
            .Content.ReadFromJsonAsync<List<StockMovementResponse>>();
        movements.Should().ContainSingle(m => m.MovementType == "GoodsReceipt" && m.Quantity == 100);

        var updatedPo = await (await Client.GetAsync($"/procurement/orders/{poId}"))
            .Content.ReadFromJsonAsync<PurchaseOrderDetailResponse>();
        updatedPo!.Status.Should().Be("Received");
    }

    [Fact(Skip = "MassTransit bus outbox interception requires OutboxDbContext to share the same physical connection as the module DbContext. The per-schema connection string architecture prevents this in WebApplicationFactory. Outbox delivery is verified via the E2E .http flow in the development environment.")]
    public async Task ReceiveGoods_FinanceCostEntryCreatedViaOutbox()
    {
        var productId = await Seed.CreateProductAsync("COMP-RG-002", "Raw Steel", "KG");
        var warehouseId = await Seed.CreateWarehouseAsync("WH-RG-002", "Receiving Warehouse 2", "Berlin, Germany");
        await Seed.CreateStockItemAsync(productId, warehouseId, "KG");
        var supplierId = await Seed.CreateSupplierAsync("SUP-RG-002", "Steel Supplier 2", "rg2@steel.de");

        var poId = await Seed.CreatePurchaseOrderAsync(
            supplierId, productId, "COMP-RG-002", "Raw Steel",
            quantity: 50, unitCode: "KG", unitPrice: 2.50m);

        await Seed.ConfirmPurchaseOrderAsync(poId);

        var po = await (await Client.GetAsync($"/procurement/orders/{poId}"))
            .Content.ReadFromJsonAsync<PurchaseOrderDetailResponse>();
        var lineId = po!.Lines[0].Id;

        await Seed.ReceiveGoodsAsync(poId, lineId, warehouseId, quantity: 50);

        await WaitHelper.WaitForConditionAsync(
            async () =>
            {
                var response = await Client.GetAsync($"/finance/cost-entries/by-source/{poId}");
                if (!response.IsSuccessStatusCode) { return false; }
                var entries = await response.Content.ReadFromJsonAsync<List<CostEntryResponse>>();
                return entries?.Any(e => e.SourceType == "GoodsReceipt") == true;
            },
            timeout: TimeSpan.FromSeconds(30),
            failureMessage: "Finance cost entry for GoodsReceipt was not created within timeout.");

        var costEntries = await (await Client.GetAsync($"/finance/cost-entries/by-source/{poId}"))
            .Content.ReadFromJsonAsync<List<CostEntryResponse>>();

        costEntries.Should().ContainSingle(e =>
            e.SourceType == "GoodsReceipt" &&
            e.Amount > 0 &&
            !e.IsPendingCosting);
    }

    private sealed record PurchaseOrderDetailResponse(Guid Id, string Status, List<PurchaseOrderLineResponse> Lines);
    private sealed record PurchaseOrderLineResponse(Guid Id, Guid ProductId, decimal Quantity);
    private sealed record StockMovementResponse(Guid Id, string MovementType, decimal Quantity);
    private sealed record CostEntryResponse(Guid Id, string SourceType, decimal Amount, bool IsPendingCosting);
}