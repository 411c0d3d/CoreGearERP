using System.Net;
using System.Net.Http.Json;
using CoreGearERP.Tests.Infrastructure;
using CoreGearERP.Tests.Infrastructure.Fixtures;
using CoreGearERP.Tests.Infrastructure.Helpers;
using FluentAssertions;
using Xunit;

namespace CoreGearERP.Tests.Modules.Sales;

/// <summary>Integration tests for sales customers, order lifecycle, and shipment with Finance outbox side effect.</summary>
public sealed class SalesOrdersTests : IntegrationTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SalesOrdersTests"/> class.
    /// </summary>
    /// <param name="fixture">The shared integration test fixture.</param>
    public SalesOrdersTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task CreateCustomer_ValidCommand_Returns201WithId()
    {
        var response = await Client.PostAsJsonAsync("/sales/customers", new
        {
            code = "CUST-001",
            name = "Test Customer GmbH",
            contactEmail = "orders@customer.de",
            contactPhone = "+49 30 11111111",
            address = "Kundenstraße 1, 10115 Berlin",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<IdResponse>();
        body!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetCustomers_AfterCreation_ReturnsCreatedCustomer()
    {
        await Seed.CreateCustomerAsync("CUST-GET-001", "Get Customer GmbH", "get@customer.de");

        var response = await Client.GetAsync("/sales/customers");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<List<CustomerResponse>>();
        body.Should().ContainSingle(c => c.Code == "CUST-GET-001");
    }

    [Fact]
    public async Task CreateSalesOrder_ValidCommand_Returns201InDraftStatus()
    {
        var productId = await Seed.CreateProductAsync("PROD-SO-001", "Finished Assembly", "PCS");
        var customerId = await Seed.CreateCustomerAsync("CUST-SO-001", "Test Customer", "so@customer.de");

        var salesOrderId = await Seed.CreateSalesOrderAsync(
            customerId, productId, "PROD-SO-001", "Finished Assembly",
            quantity: 5, unitCode: "PCS", unitPrice: 25.00m);

        var response = await Client.GetAsync($"/sales/orders/{salesOrderId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<SalesOrderDetailResponse>();
        body!.Status.Should().Be("Draft");
        body.Lines.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetSalesOrders_FilterByStatus_ReturnsMatchingOrders()
    {
        var productId = await Seed.CreateProductAsync("PROD-SO-FILT", "Finished Assembly", "PCS");
        var customerId = await Seed.CreateCustomerAsync("CUST-SO-FILT", "Filter Customer", "filt@customer.de");

        var salesOrderId = await Seed.CreateSalesOrderAsync(
            customerId, productId, "PROD-SO-FILT", "Finished Assembly",
            quantity: 2, unitCode: "PCS", unitPrice: 10.00m);

        var response = await Client.GetAsync("/sales/orders?status=Draft");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<List<SalesOrderResponse>>();
        body.Should().Contain(o => o.Id == salesOrderId);
    }

    [Fact]
    public async Task ConfirmSalesOrder_ReservesStock()
    {
        var productId = await Seed.CreateProductAsync("PROD-SO-002", "Finished Assembly", "PCS");
        var warehouseId = await Seed.CreateWarehouseAsync("WH-SO-002", "Sales Warehouse", "Berlin, Germany");
        await Seed.CreateStockItemAsync(productId, warehouseId, "PCS");

        await SeedFinishedStock(productId, "PROD-SO-002", warehouseId, quantity: 10);

        var customerId = await Seed.CreateCustomerAsync("CUST-SO-002", "Test Customer 2", "so2@customer.de");
        var salesOrderId = await Seed.CreateSalesOrderAsync(
            customerId, productId, "PROD-SO-002", "Finished Assembly",
            quantity: 5, unitCode: "PCS", unitPrice: 25.00m);

        await Seed.ConfirmSalesOrderAsync(salesOrderId, warehouseId);

        var stockItems = await (await Client.GetAsync("/inventory/stock-items"))
            .Content.ReadFromJsonAsync<List<StockItemResponse>>();
        var item = stockItems!.First(s => s.ProductCode == "PROD-SO-002");

        item.QuantityOnHand.Should().Be(10);
        item.QuantityReserved.Should().Be(5);
        item.QuantityAvailable.Should().Be(5);
    }

    [Fact]
    public async Task ShipSalesOrder_StockReducedAndFinanceCostEntryCreatedViaOutbox()
    {
        var productId = await Seed.CreateProductAsync("PROD-SHIP-001", "Finished Assembly", "PCS");
        var warehouseId = await Seed.CreateWarehouseAsync("WH-SHIP-001", "Ship Warehouse", "Berlin, Germany");
        await Seed.CreateStockItemAsync(productId, warehouseId, "PCS");

        await SeedFinishedStock(productId, "PROD-SHIP-001", warehouseId, quantity: 10);

        var customerId = await Seed.CreateCustomerAsync("CUST-SHIP-001", "Ship Customer", "ship@customer.de");
        var salesOrderId = await Seed.CreateSalesOrderAsync(
            customerId, productId, "PROD-SHIP-001", "Finished Assembly",
            quantity: 5, unitCode: "PCS", unitPrice: 25.00m);

        await Seed.ConfirmSalesOrderAsync(salesOrderId, warehouseId);

        var soDetail = await (await Client.GetAsync($"/sales/orders/{salesOrderId}"))
            .Content.ReadFromJsonAsync<SalesOrderDetailResponse>();
        var lineId = soDetail!.Lines[0].Id;

        var shipmentId = await Seed.ShipSalesOrderAsync(
            salesOrderId, lineId, productId, "PROD-SHIP-001",
            warehouseId, quantity: 5, unitCode: "PCS");

        var updatedSo = await (await Client.GetAsync($"/sales/orders/{salesOrderId}"))
            .Content.ReadFromJsonAsync<SalesOrderDetailResponse>();
        updatedSo!.Status.Should().Be("Shipped");
        updatedSo.Lines[0].QuantityShipped.Should().Be(5);

        var stockItems = await (await Client.GetAsync("/inventory/stock-items"))
            .Content.ReadFromJsonAsync<List<StockItemResponse>>();
        var item = stockItems!.First(s => s.ProductCode == "PROD-SHIP-001");

        item.QuantityOnHand.Should().Be(5);
        item.QuantityReserved.Should().Be(0);

        await WaitHelper.WaitForConditionAsync(
            async () =>
            {
                var response = await Client.GetAsync($"/finance/cost-entries/by-source/{shipmentId}");
                if (!response.IsSuccessStatusCode) { return false; }
                var entries = await response.Content.ReadFromJsonAsync<List<CostEntryResponse>>();
                return entries?.Any(e => e.SourceType == "Shipment") == true;
            },
            timeout: TimeSpan.FromSeconds(30),
            failureMessage: "Finance cost entry for Shipment was not created within timeout.");

        var costEntries = await (await Client.GetAsync($"/finance/cost-entries/by-source/{shipmentId}"))
            .Content.ReadFromJsonAsync<List<CostEntryResponse>>();

        costEntries.Should().ContainSingle(e =>
            e.SourceType == "Shipment" &&
            e.Amount > 0 &&
            !e.IsPendingCosting);
    }

    [Fact]
    public async Task GetSalesOrderById_NonExistent_Returns404()
    {
        var response = await Client.GetAsync($"/sales/orders/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Seeds finished goods stock via a minimal procurement and production cycle.
    /// </summary>
    private async Task SeedFinishedStock(Guid finishedProductId, string finishedProductCode, Guid warehouseId,
        decimal quantity)
    {
        var componentCode = $"COMP-SEED-{Guid.NewGuid():N}"[..20];
        var componentProductId = await Seed.CreateProductAsync(componentCode, "Raw Component", "KG");
        await Seed.CreateStockItemAsync(componentProductId, warehouseId, "KG");

        var supplierId = await Seed.CreateSupplierAsync($"S-{Guid.NewGuid():N}"[..15], "Seed Supplier", $"{Guid.NewGuid():N}@seed.de");
        var poId = await Seed.CreatePurchaseOrderAsync(
            supplierId, componentProductId, componentCode, "Raw Component",
            quantity: quantity * 5, unitCode: "KG", unitPrice: 1.00m);

        await Seed.ConfirmPurchaseOrderAsync(poId);
        var poDetail = await (await Client.GetAsync($"/procurement/orders/{poId}"))
            .Content.ReadFromJsonAsync<PurchaseOrderDetailResponse>();
        await Seed.ReceiveGoodsAsync(poId, poDetail!.Lines[0].Id, warehouseId, quantity: quantity * 5);

        var workCenterId = await Seed.CreateWorkCenterAsync($"WC-{Guid.NewGuid():N}"[..12], "Seed Work Center");
        var bomId = await Seed.CreateBillOfMaterialsAsync(
            finishedProductId, finishedProductCode, finishedProductCode,
            componentProductId, componentCode, "Raw Component",
            componentQuantity: 5, componentUnitCode: "KG");

        var productionOrderId = await Seed.CreateProductionOrderAsync(bomId, workCenterId, plannedQuantity: quantity, unitCode: "PCS");

        await Seed.ConfirmProductionOrderAsync(productionOrderId, componentProductId, warehouseId);
        await Seed.StartProductionOrderAsync(productionOrderId);
        await Seed.CompleteProductionOrderAsync(productionOrderId, warehouseId, actualQuantity: quantity, componentProductId, warehouseId);
    }

    private sealed record IdResponse(Guid Id);

    private sealed record CustomerResponse(Guid Id, string Code, string Name);

    private sealed record SalesOrderResponse(Guid Id, string Status);

    private sealed record SalesOrderDetailResponse(Guid Id, string Status, List<SalesOrderLineResponse> Lines);

    private sealed record SalesOrderLineResponse(
        Guid Id,
        Guid ProductId,
        decimal QuantityOrdered,
        decimal QuantityShipped);

    private sealed record StockItemResponse(
        Guid Id,
        string ProductCode,
        decimal QuantityOnHand,
        decimal QuantityReserved,
        decimal QuantityAvailable);

    private sealed record CostEntryResponse(Guid Id, string SourceType, decimal Amount, bool IsPendingCosting);

    private sealed record PurchaseOrderDetailResponse(Guid Id, string Status, List<PurchaseOrderLineResponse> Lines);

    private sealed record PurchaseOrderLineResponse(Guid Id, Guid ProductId, decimal Quantity);
}