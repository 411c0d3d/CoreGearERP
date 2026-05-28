using System.Net;
using System.Net.Http.Json;
using CoreGearERP.Tests.Infrastructure;
using CoreGearERP.Tests.Infrastructure.Fixtures;
using FluentAssertions;
using Xunit;

namespace CoreGearERP.Tests.Modules.Procurement;

/// <summary>
/// Integration tests for procurement suppliers and purchase order lifecycle.
/// </summary>
public sealed class PurchaseOrdersTests : IntegrationTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PurchaseOrdersTests"/> class.
    /// </summary>
    /// <param name="fixture">The shared integration test fixture.</param>
    public PurchaseOrdersTests(IntegrationTestFixture fixture) 
        : base(fixture) { }

    [Fact]
    public async Task CreateSupplier_ValidCommand_Returns201WithId()
    {
        var response = await Client.PostAsJsonAsync("/procurement/suppliers", new
        {
            code = "SUP-001",
            name = "Steel Supplier GmbH",
            contactEmail = "orders@steel-supplier.de",
            contactPhone = "+49 30 00000000",
            address = "Teststraße 1, 10115 Berlin",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<IdResponse>();
        body!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetSuppliers_AfterCreation_ReturnsCreatedSupplier()
    {
        await Seed.CreateSupplierAsync("SUP-GET-001", "Get Supplier GmbH", "get@steel.de");

        var response = await Client.GetAsync("/procurement/suppliers");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<List<SupplierResponse>>();
        body.Should().ContainSingle(s => s.Code == "SUP-GET-001");
    }

    [Fact]
    public async Task CreatePurchaseOrder_ValidCommand_Returns201InDraftStatus()
    {
        var productId = await Seed.CreateProductAsync("COMP-PO-001", "Raw Steel", "KG");
        var supplierId = await Seed.CreateSupplierAsync("SUP-PO-001", "Steel Supplier GmbH", "orders@steel.de");

        var poId = await Seed.CreatePurchaseOrderAsync(
            supplierId, productId, "COMP-PO-001", "Raw Steel",
            quantity: 100, unitCode: "KG", unitPrice: 2.50m);

        var response = await Client.GetAsync($"/procurement/orders/{poId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<PurchaseOrderDetailResponse>();
        body!.Status.Should().Be("Draft");
        body.Lines.Should().HaveCount(1);
    }

    [Fact]
    public async Task ConfirmPurchaseOrder_FromDraft_StatusBecomesConfirmed()
    {
        var productId = await Seed.CreateProductAsync("COMP-PO-002", "Raw Steel", "KG");
        var supplierId = await Seed.CreateSupplierAsync("SUP-PO-002", "Steel Supplier GmbH", "orders@steel2.de");
        var poId = await Seed.CreatePurchaseOrderAsync(
            supplierId, productId, "COMP-PO-002", "Raw Steel",
            quantity: 50, unitCode: "KG", unitPrice: 3.00m);

        await Seed.ConfirmPurchaseOrderAsync(poId);

        var body = await (await Client.GetAsync($"/procurement/orders/{poId}"))
            .Content.ReadFromJsonAsync<PurchaseOrderDetailResponse>();

        body!.Status.Should().Be("Confirmed");
    }

    [Fact]
    public async Task GetPurchaseOrders_FilterByStatus_ReturnsMatchingOrders()
    {
        var productId = await Seed.CreateProductAsync("COMP-PO-003", "Raw Steel", "KG");
        var supplierId = await Seed.CreateSupplierAsync("SUP-PO-003", "Steel Supplier GmbH", "orders@steel3.de");

        var poId = await Seed.CreatePurchaseOrderAsync(
            supplierId, productId, "COMP-PO-003", "Raw Steel",
            quantity: 20, unitCode: "KG", unitPrice: 2.00m);

        await Seed.ConfirmPurchaseOrderAsync(poId);

        var response = await Client.GetAsync("/procurement/orders?status=Confirmed");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<List<PurchaseOrderResponse>>();
        body.Should().Contain(o => o.Id == poId);
    }

    [Fact]
    public async Task GetPurchaseOrderById_NonExistent_Returns404()
    {
        var response = await Client.GetAsync($"/procurement/orders/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private sealed record IdResponse(Guid Id);
    private sealed record SupplierResponse(Guid Id, string Code, string Name);
    private sealed record PurchaseOrderResponse(Guid Id, string Status);
    private sealed record PurchaseOrderDetailResponse(Guid Id, string Status, List<PurchaseOrderLineResponse> Lines);
    private sealed record PurchaseOrderLineResponse(Guid Id, Guid ProductId, decimal Quantity);
}