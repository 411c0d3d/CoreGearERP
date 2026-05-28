using System.Net;
using System.Net.Http.Json;
using CoreGearERP.Tests.Infrastructure;
using CoreGearERP.Tests.Infrastructure.Fixtures;
using CoreGearERP.Tests.Infrastructure.Helpers;
using FluentAssertions;
using Xunit;

namespace CoreGearERP.Tests.Modules.Production;

/// <summary>
/// Integration tests for production work centers, bills of materials, and production order lifecycle.
/// </summary>
public sealed class ProductionOrdersTests : IntegrationTestBase
{
    public ProductionOrdersTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task CreateWorkCenter_ValidCommand_Returns201WithId()
    {
        var response = await Client.PostAsJsonAsync("/production/work-centers", new
        {
            code = "WC-001",
            name = "Assembly Line A",
            capacityPerHour = 75m,
            description = "Primary assembly line",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<IdResponse>();
        body!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetWorkCenters_AfterCreation_ReturnsCreatedWorkCenter()
    {
        await Seed.CreateWorkCenterAsync("WC-GET-001", "Get Assembly Line");

        var response = await Client.GetAsync("/production/work-centers");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<List<WorkCenterResponse>>();
        body.Should().ContainSingle(w => w.Code == "WC-GET-001");
    }

    [Fact]
    public async Task CreateBillOfMaterials_ValidCommand_Returns201WithId()
    {
        var finishedProductId = await Seed.CreateProductAsync("PROD-BOM-001", "Finished Assembly", "PCS");
        var componentProductId = await Seed.CreateProductAsync("COMP-BOM-001", "Raw Component", "KG");

        var response = await Client.PostAsJsonAsync("/production/bills-of-materials", new
        {
            finishedProductId,
            finishedProductCode = "PROD-BOM-001",
            finishedProductName = "Finished Assembly",
            version = "1.0",
            notes = string.Empty,
            lines = new[]
            {
                new
                {
                    componentProductId,
                    componentProductCode = "COMP-BOM-001",
                    componentProductName = "Raw Component",
                    quantity = 5m,
                    unitCode = "KG",
                },
            },
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<IdResponse>();
        body!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetBillsOfMaterials_AfterCreation_ReturnsCreatedBom()
    {
        var finishedProductId = await Seed.CreateProductAsync("PROD-BOM-GET-001", "Finished Assembly", "PCS");
        var componentProductId = await Seed.CreateProductAsync("COMP-BOM-GET-001", "Raw Component", "KG");

        var bomId = await Seed.CreateBillOfMaterialsAsync(
            finishedProductId, "PROD-BOM-GET-001", "Finished Assembly",
            componentProductId, "COMP-BOM-GET-001", "Raw Component",
            componentQuantity: 3, componentUnitCode: "KG");

        var response = await Client.GetAsync("/production/bills-of-materials");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<List<BillOfMaterialsResponse>>();
        body.Should().Contain(b => b.Id == bomId);
    }

    [Fact]
    public async Task GetProductionOrders_FilterByStatus_ReturnsMatchingOrders()
    {
        var finishedProductId = await Seed.CreateProductAsync("PROD-PO-FILT", "Finished Assembly", "PCS");
        var componentProductId = await Seed.CreateProductAsync("COMP-PO-FILT", "Raw Component", "KG");
        var warehouseId = await Seed.CreateWarehouseAsync("WH-PO-FILT", "Filter Warehouse", "Berlin, Germany");

        await Seed.CreateStockItemAsync(componentProductId, warehouseId, "KG");
        await Seed.CreateStockItemAsync(finishedProductId, warehouseId, "PCS");

        var supplierId = await Seed.CreateSupplierAsync("SUP-PO-FILT", "Filter Supplier", "filt@steel.de");
        var poId = await Seed.CreatePurchaseOrderAsync(
            supplierId, componentProductId, "COMP-PO-FILT", "Raw Component",
            quantity: 50, unitCode: "KG", unitPrice: 1.00m);
        await Seed.ConfirmPurchaseOrderAsync(poId);
        var poDetail = await (await Client.GetAsync($"/procurement/orders/{poId}"))
            .Content.ReadFromJsonAsync<PurchaseOrderDetailResponse>();
        await Seed.ReceiveGoodsAsync(poId, poDetail!.Lines[0].Id, warehouseId, quantity: 50);

        var workCenterId = await Seed.CreateWorkCenterAsync("WC-PO-FILT", "Filter Work Center");
        var bomId = await Seed.CreateBillOfMaterialsAsync(
            finishedProductId, "PROD-PO-FILT", "Finished Assembly",
            componentProductId, "COMP-PO-FILT", "Raw Component",
            componentQuantity: 5, componentUnitCode: "KG");

        var productionOrderId = await Seed.CreateProductionOrderAsync(bomId, workCenterId, plannedQuantity: 5, unitCode: "PCS");
        await Seed.ConfirmProductionOrderAsync(productionOrderId, componentProductId, warehouseId);

        var response = await Client.GetAsync("/production/orders?status=Confirmed");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<List<ProductionOrderResponse>>();
        body.Should().Contain(o => o.Id == productionOrderId);
    }

    [Fact]
    public async Task ProductionOrder_FullLifecycle_StockConsumedAndFinishedGoodsAdded()
    {
        var finishedProductId = await Seed.CreateProductAsync("PROD-PO-001", "Finished Steel Assembly", "PCS");
        var componentProductId = await Seed.CreateProductAsync("COMP-PO-001", "Raw Steel Component", "KG");
        var warehouseId = await Seed.CreateWarehouseAsync("WH-PO-001", "Production Warehouse", "Berlin, Germany");

        await Seed.CreateStockItemAsync(componentProductId, warehouseId, "KG");
        await Seed.CreateStockItemAsync(finishedProductId, warehouseId, "PCS");

        var supplierId = await Seed.CreateSupplierAsync("SUP-PO-001", "Steel Supplier", "po@steel.de");
        var poId = await Seed.CreatePurchaseOrderAsync(
            supplierId, componentProductId, "COMP-PO-001", "Raw Steel Component",
            quantity: 100, unitCode: "KG", unitPrice: 2.50m);

        await Seed.ConfirmPurchaseOrderAsync(poId);
        var poDetail = await (await Client.GetAsync($"/procurement/orders/{poId}"))
            .Content.ReadFromJsonAsync<PurchaseOrderDetailResponse>();
        await Seed.ReceiveGoodsAsync(poId, poDetail!.Lines[0].Id, warehouseId, quantity: 100);

        var workCenterId = await Seed.CreateWorkCenterAsync("WC-PO-001", "Assembly Line");
        var bomId = await Seed.CreateBillOfMaterialsAsync(
            finishedProductId, "PROD-PO-001", "Finished Steel Assembly",
            componentProductId, "COMP-PO-001", "Raw Steel Component",
            componentQuantity: 5, componentUnitCode: "KG");

        var productionOrderId = await Seed.CreateProductionOrderAsync(bomId, workCenterId, plannedQuantity: 10, unitCode: "PCS");

        await Seed.ConfirmProductionOrderAsync(productionOrderId, componentProductId, warehouseId);
        await Seed.StartProductionOrderAsync(productionOrderId);
        await Seed.CompleteProductionOrderAsync(productionOrderId, warehouseId, actualQuantity: 10, componentProductId, warehouseId);

        var stockItems = (await (await Client.GetAsync("/inventory/stock-items"))
            .Content.ReadFromJsonAsync<List<StockItemResponse>>())!;

        stockItems.First(s => s.ProductCode == "COMP-PO-001").QuantityOnHand.Should().Be(50);
        stockItems.First(s => s.ProductCode == "PROD-PO-001").QuantityOnHand.Should().Be(10);
    }

    [Fact(Skip = "MassTransit bus outbox publish interception requires shared transaction between module and OutboxDbContext. WebApplicationFactory test host does not support cross-schema shared transactions reliably. Outbox delivery tested via E2E flow in development environment.")]
    public async Task CompleteProductionOrder_FinanceCostEntryCreatedViaOutbox()
    {
        var finishedProductId = await Seed.CreateProductAsync("PROD-FIN-001", "Finished Assembly", "PCS");
        var componentProductId = await Seed.CreateProductAsync("COMP-FIN-001", "Raw Component", "KG");
        var warehouseId = await Seed.CreateWarehouseAsync("WH-FIN-001", "Finance Test Warehouse", "Berlin, Germany");

        await Seed.CreateStockItemAsync(componentProductId, warehouseId, "KG");
        await Seed.CreateStockItemAsync(finishedProductId, warehouseId, "PCS");

        var supplierId = await Seed.CreateSupplierAsync("SUP-FIN-001", "Finance Supplier", "fin@steel.de");
        var poId = await Seed.CreatePurchaseOrderAsync(
            supplierId, componentProductId, "COMP-FIN-001", "Raw Component",
            quantity: 50, unitCode: "KG", unitPrice: 2.00m);

        await Seed.ConfirmPurchaseOrderAsync(poId);
        var poDetail = await (await Client.GetAsync($"/procurement/orders/{poId}"))
            .Content.ReadFromJsonAsync<PurchaseOrderDetailResponse>();
        await Seed.ReceiveGoodsAsync(poId, poDetail!.Lines[0].Id, warehouseId, quantity: 50);

        var workCenterId = await Seed.CreateWorkCenterAsync("WC-FIN-001", "Finance Work Center");
        var bomId = await Seed.CreateBillOfMaterialsAsync(
            finishedProductId, "PROD-FIN-001", "Finished Assembly",
            componentProductId, "COMP-FIN-001", "Raw Component",
            componentQuantity: 5, componentUnitCode: "KG");

        var productionOrderId = await Seed.CreateProductionOrderAsync(bomId, workCenterId, plannedQuantity: 5, unitCode: "PCS");

        await Seed.ConfirmProductionOrderAsync(productionOrderId, componentProductId, warehouseId);
        await Seed.StartProductionOrderAsync(productionOrderId);
        await Seed.CompleteProductionOrderAsync(productionOrderId, warehouseId, actualQuantity: 5, componentProductId, warehouseId);

        await WaitHelper.WaitForConditionAsync(
            async () =>
            {
                var response = await Client.GetAsync($"/finance/cost-entries/by-source/{productionOrderId}");
                if (!response.IsSuccessStatusCode) { return false; }
                var entries = await response.Content.ReadFromJsonAsync<List<CostEntryResponse>>();
                return entries?.Any(e => e.SourceType == "ProductionOrder") == true;
            },
            timeout: TimeSpan.FromSeconds(30),
            failureMessage: "Finance cost entry for ProductionOrder was not created within timeout.");

        var costEntries = await (await Client.GetAsync($"/finance/cost-entries/by-source/{productionOrderId}"))
            .Content.ReadFromJsonAsync<List<CostEntryResponse>>();

        costEntries.Should().ContainSingle(e =>
            e.SourceType == "ProductionOrder" &&
            e.IsPendingCosting);
    }

    private sealed record IdResponse(Guid Id);
    private sealed record WorkCenterResponse(Guid Id, string Code, string Name);
    private sealed record BillOfMaterialsResponse(Guid Id, Guid FinishedProductId);
    private sealed record ProductionOrderResponse(Guid Id, string Status);
    private sealed record PurchaseOrderDetailResponse(Guid Id, string Status, List<PurchaseOrderLineResponse> Lines);
    private sealed record PurchaseOrderLineResponse(Guid Id, Guid ProductId, decimal Quantity);
    private sealed record StockItemResponse(Guid Id, string ProductCode, decimal QuantityOnHand);
    private sealed record CostEntryResponse(Guid Id, string SourceType, decimal Amount, bool IsPendingCosting);
}