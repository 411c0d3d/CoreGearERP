using System.Net.Http.Json;

namespace CoreGearERP.Tests.Infrastructure.Helpers;

/// <summary>
/// Typed HTTP seed helpers covering the full E2E operational chain.
/// </summary>
public sealed class SeedHelper
{
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeedHelper"/> class.
    /// </summary>
    public SeedHelper(HttpClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Creates a product and returns its id.
    /// </summary>
    public async Task<Guid> CreateProductAsync(string code, string name, string unitCode, string? description = null)
    {
        var response = await _client.PostAsJsonAsync("/inventory/products", new
        {
            code,
            name,
            unitCode,
            description = description ?? string.Empty,
        });

        await EnsureSuccessAsync(response, "CreateProduct");
        return (await response.Content.ReadFromJsonAsync<IdResponse>())!.Id;
    }

    /// <summary>
    /// Creates a warehouse and returns its id.
    /// </summary>
    public async Task<Guid> CreateWarehouseAsync(string code, string name, string location)
    {
        var response = await _client.PostAsJsonAsync("/inventory/warehouses", new { code, name, location });
        await EnsureSuccessAsync(response, "CreateWarehouse");
        return (await response.Content.ReadFromJsonAsync<IdResponse>())!.Id;
    }

    /// <summary>
    /// Creates a stock item and returns its id.
    /// </summary>
    public async Task<Guid> CreateStockItemAsync(Guid productId, Guid warehouseId, string unitCode)
    {
        var response = await _client.PostAsJsonAsync("/inventory/stock-items", new { productId, warehouseId, unitCode });
        await EnsureSuccessAsync(response, "CreateStockItem");
        return (await response.Content.ReadFromJsonAsync<IdResponse>())!.Id;
    }

    /// <summary>
    /// Creates a supplier and returns its id.
    /// </summary>
    public async Task<Guid> CreateSupplierAsync(string code, string name, string contactEmail)
    {
        var response = await _client.PostAsJsonAsync("/procurement/suppliers", new
        {
            code,
            name,
            contactEmail,
            contactPhone = "+49 30 00000000",
            address = "Teststraße 1, 10115 Berlin",
        });

        await EnsureSuccessAsync(response, "CreateSupplier");
        return (await response.Content.ReadFromJsonAsync<IdResponse>())!.Id;
    }

    /// <summary>
    /// Creates a purchase order with a single line and returns its id.
    /// </summary>
    public async Task<Guid> CreatePurchaseOrderAsync(
        Guid supplierId,
        Guid productId,
        string productCode,
        string productName,
        decimal quantity,
        string unitCode,
        decimal unitPrice,
        string currencyCode = "EUR")
    {
        var response = await _client.PostAsJsonAsync("/procurement/orders", new
        {
            supplierId,
            notes = "Integration test PO",
            lines = new[]
            {
                new { productId, productCode, productName, quantity, unitCode, unitPrice, currencyCode },
            },
        });

        await EnsureSuccessAsync(response, "CreatePurchaseOrder");
        return (await response.Content.ReadFromJsonAsync<IdResponse>())!.Id;
    }

    /// <summary>
    /// Confirms a purchase order.
    /// </summary>
    public async Task ConfirmPurchaseOrderAsync(Guid purchaseOrderId)
    {
        var response = await _client.PutAsync($"/procurement/orders/{purchaseOrderId}/confirm", null);
        await EnsureSuccessAsync(response, "ConfirmPurchaseOrder");
    }

    /// <summary>
    /// Receives goods against a purchase order line.
    /// </summary>
    public async Task ReceiveGoodsAsync(Guid purchaseOrderId, Guid purchaseOrderLineId, Guid warehouseId, decimal quantity)
    {
        var response = await _client.PostAsJsonAsync($"/procurement/orders/{purchaseOrderId}/receive", new
        {
            purchaseOrderId,
            purchaseOrderLineId,
            warehouseId,
            quantity,
        });

        await EnsureSuccessAsync(response, "ReceiveGoods");
    }

    /// <summary>
    /// Creates a work center and returns its id.
    /// </summary>
    public async Task<Guid> CreateWorkCenterAsync(string code, string name)
    {
        var response = await _client.PostAsJsonAsync("/production/work-centers", new
        {
            code,
            name,
            capacityPerHour = 50m,
            description = "Integration test work center",
        });

        await EnsureSuccessAsync(response, "CreateWorkCenter");
        return (await response.Content.ReadFromJsonAsync<IdResponse>())!.Id;
    }

    /// <summary>
    /// Creates a bill of materials with a single component line and returns its id.
    /// </summary>
    public async Task<Guid> CreateBillOfMaterialsAsync(
        Guid finishedProductId,
        string finishedProductCode,
        string finishedProductName,
        Guid componentProductId,
        string componentProductCode,
        string componentProductName,
        decimal componentQuantity,
        string componentUnitCode)
    {
        var response = await _client.PostAsJsonAsync("/production/bills-of-materials", new
        {
            finishedProductId,
            finishedProductCode,
            finishedProductName,
            version = "1.0",
            notes = string.Empty,
            lines = new[]
            {
                new
                {
                    componentProductId,
                    componentProductCode,
                    componentProductName,
                    quantity = componentQuantity,
                    unitCode = componentUnitCode,
                },
            },
        });

        await EnsureSuccessAsync(response, "CreateBillOfMaterials");
        return (await response.Content.ReadFromJsonAsync<IdResponse>())!.Id;
    }

    /// <summary>
    /// Creates a production order and returns its id.
    /// </summary>
    public async Task<Guid> CreateProductionOrderAsync(
        Guid billOfMaterialsId,
        Guid workCenterId,
        decimal plannedQuantity,
        string unitCode)
    {
        var response = await _client.PostAsJsonAsync("/production/orders", new
        {
            billOfMaterialsId,
            workCenterId,
            plannedQuantity,
            unitCode,
            notes = "Integration test production order",
        });

        await EnsureSuccessAsync(response, "CreateProductionOrder");
        return (await response.Content.ReadFromJsonAsync<IdResponse>())!.Id;
    }

    /// <summary>
    /// Confirms a production order.
    /// </summary>
    public async Task ConfirmProductionOrderAsync(Guid productionOrderId, Guid componentProductId, Guid warehouseId)
    {
        var response = await _client.PutAsJsonAsync($"/production/orders/{productionOrderId}/confirm", new
        {
            productionOrderId,
            componentWarehouses = new[]
            {
                new { componentProductId, warehouseId },
            },
        });

        await EnsureSuccessAsync(response, "ConfirmProductionOrder");
    }

    /// <summary>
    /// Starts a production order.
    /// </summary>
    public async Task StartProductionOrderAsync(Guid productionOrderId)
    {
        var response = await _client.PutAsync($"/production/orders/{productionOrderId}/start", null);
        await EnsureSuccessAsync(response, "StartProductionOrder");
    }

    /// <summary>
    /// Completes a production order.
    /// </summary>
    public async Task CompleteProductionOrderAsync(
        Guid productionOrderId,
        Guid finishedGoodsWarehouseId,
        decimal actualQuantity,
        Guid componentProductId,
        Guid componentWarehouseId)
    {
        var response = await _client.PutAsJsonAsync($"/production/orders/{productionOrderId}/complete", new
        {
            productionOrderId,
            finishedGoodsWarehouseId,
            actualQuantity,
            componentWarehouses = new[]
            {
                new { componentProductId, warehouseId = componentWarehouseId },
            },
        });

        await EnsureSuccessAsync(response, "CompleteProductionOrder");
    }

    /// <summary>
    /// Creates a customer and returns its id.
    /// </summary>
    public async Task<Guid> CreateCustomerAsync(string code, string name, string contactEmail)
    {
        var response = await _client.PostAsJsonAsync("/sales/customers", new
        {
            code,
            name,
            contactEmail,
            contactPhone = "+49 30 11111111",
            address = "Kundenstraße 1, 10115 Berlin",
        });

        await EnsureSuccessAsync(response, "CreateCustomer");
        return (await response.Content.ReadFromJsonAsync<IdResponse>())!.Id;
    }

    /// <summary>
    /// Creates a sales order with a single line and returns its id.
    /// </summary>
    public async Task<Guid> CreateSalesOrderAsync(
        Guid customerId,
        Guid productId,
        string productCode,
        string productName,
        decimal quantity,
        string unitCode,
        decimal unitPrice,
        string currencyCode = "EUR")
    {
        var response = await _client.PostAsJsonAsync("/sales/orders", new
        {
            customerId,
            notes = "Integration test sales order",
            lines = new[]
            {
                new { productId, productCode, productName, quantity, unitCode, unitPrice, currencyCode },
            },
        });

        await EnsureSuccessAsync(response, "CreateSalesOrder");
        return (await response.Content.ReadFromJsonAsync<IdResponse>())!.Id;
    }

    /// <summary>
    /// Confirms a sales order, reserving stock from the given warehouse.
    /// </summary>
    public async Task ConfirmSalesOrderAsync(Guid salesOrderId, Guid warehouseId)
    {
        var response = await _client.PutAsJsonAsync($"/sales/orders/{salesOrderId}/confirm", new
        {
            salesOrderId,
            warehouseId,
        });

        await EnsureSuccessAsync(response, "ConfirmSalesOrder");
    }

    /// <summary>
    /// Ships a sales order line and returns the shipment id.
    /// </summary>
    public async Task<Guid> ShipSalesOrderAsync(
        Guid salesOrderId,
        Guid salesOrderLineId,
        Guid productId,
        string productCode,
        Guid warehouseId,
        decimal quantity,
        string unitCode)
    {
        var response = await _client.PostAsJsonAsync($"/sales/orders/{salesOrderId}/ship", new
        {
            salesOrderId,
            warehouseId,
            notes = "Integration test shipment",
            lines = new[]
            {
                new { salesOrderLineId, productId, productCode, quantity, unitCode },
            },
        });

        await EnsureSuccessAsync(response, "ShipSalesOrder");
        return (await response.Content.ReadFromJsonAsync<IdResponse>())!.Id;
    }

    /// <summary>
    /// Throws with full response body on non-success status codes.
    /// </summary>
    private static async Task EnsureSuccessAsync(HttpResponseMessage response, string operation)
    {
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"{operation} failed {(int)response.StatusCode}: {body}");
        }
    }

    private sealed record IdResponse(Guid Id);
}