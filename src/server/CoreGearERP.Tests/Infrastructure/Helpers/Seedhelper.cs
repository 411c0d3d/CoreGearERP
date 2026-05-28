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
    /// <param name="client">The authenticated HTTP client to use for seeding.</param>
    public SeedHelper(HttpClient client)
    {
        _client = client;
    }

    // Inventory

    /// <summary>
    /// Creates a product and returns its id.
    /// </summary>
    /// <param name="code">Unique product code.</param>
    /// <param name="name">Display name of the product.</param>
    /// <param name="unitCode">Unit of measure code.</param>
    /// <param name="description">Optional description.</param>
    public async Task<Guid> CreateProductAsync(string code, string name, string unitCode, string? description = null)
    {
        var response = await _client.PostAsJsonAsync("/inventory/products", new
        {
            code,
            name,
            unitCode,
            description = description ?? string.Empty,
        });

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IdResponse>())!.Id;
    }

    /// <summary>
    /// Creates a warehouse and returns its id.
    /// </summary>
    /// <param name="code">Unique warehouse code.</param>
    /// <param name="name">Display name of the warehouse.</param>
    /// <param name="location">Physical location description.</param>
    public async Task<Guid> CreateWarehouseAsync(string code, string name, string location)
    {
        var response = await _client.PostAsJsonAsync("/inventory/warehouses", new { code, name, location });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IdResponse>())!.Id;
    }

    /// <summary>
    /// Creates a stock item and returns its id.
    /// </summary>
    /// <param name="productId">Id of the product to stock.</param>
    /// <param name="warehouseId">Id of the warehouse to stock the product in.</param>
    /// <param name="unitCode">Unit of measure code.</param>
    public async Task<Guid> CreateStockItemAsync(Guid productId, Guid warehouseId, string unitCode)
    {
        var response = await _client.PostAsJsonAsync("/inventory/stock-items", new { productId, warehouseId, unitCode });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IdResponse>())!.Id;
    }

    // Procurement

    /// <summary>
    /// Creates a supplier and returns its id.
    /// </summary>
    /// <param name="code">Unique supplier code.</param>
    /// <param name="name">Display name of the supplier.</param>
    /// <param name="contactEmail">Primary contact email address.</param>
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

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IdResponse>())!.Id;
    }

    /// <summary>
    /// Creates a purchase order with a single line and returns its id.
    /// </summary>
    /// <param name="supplierId">Id of the supplier.</param>
    /// <param name="productId">Id of the product to order.</param>
    /// <param name="productCode">Product code.</param>
    /// <param name="productName">Product display name.</param>
    /// <param name="quantity">Quantity to order.</param>
    /// <param name="unitCode">Unit of measure code.</param>
    /// <param name="unitPrice">Price per unit.</param>
    /// <param name="currencyCode">Currency code, defaults to EUR.</param>
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

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IdResponse>())!.Id;
    }

    /// <summary>
    /// Confirms a purchase order.
    /// </summary>
    /// <param name="purchaseOrderId">Id of the purchase order to confirm.</param>
    public async Task ConfirmPurchaseOrderAsync(Guid purchaseOrderId)
    {
        var response = await _client.PutAsync($"/procurement/orders/{purchaseOrderId}/confirm", null);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Receives goods against a purchase order line.
    /// </summary>
    /// <param name="purchaseOrderId">Id of the purchase order.</param>
    /// <param name="purchaseOrderLineId">Id of the line to receive against.</param>
    /// <param name="warehouseId">Id of the destination warehouse.</param>
    /// <param name="quantity">Quantity being received.</param>
    public async Task ReceiveGoodsAsync(Guid purchaseOrderId, Guid purchaseOrderLineId, Guid warehouseId, decimal quantity)
    {
        var response = await _client.PostAsJsonAsync($"/procurement/orders/{purchaseOrderId}/receive", new
        {
            purchaseOrderId,
            purchaseOrderLineId,
            warehouseId,
            quantity,
        });

        response.EnsureSuccessStatusCode();
    }

    // Production

    /// <summary>
    /// Creates a work center and returns its id.
    /// </summary>
    /// <param name="code">Unique work center code.</param>
    /// <param name="name">Display name of the work center.</param>
    public async Task<Guid> CreateWorkCenterAsync(string code, string name)
    {
        var response = await _client.PostAsJsonAsync("/production/work-centers", new
        {
            code,
            name,
            capacityPerHour = 50m,
            description = "Integration test work center",
        });

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IdResponse>())!.Id;
    }

    /// <summary>
    /// Creates a bill of materials with a single component line and returns its id.
    /// </summary>
    /// <param name="finishedProductId">Id of the finished product.</param>
    /// <param name="finishedProductCode">Code of the finished product.</param>
    /// <param name="finishedProductName">Name of the finished product.</param>
    /// <param name="componentProductId">Id of the component product.</param>
    /// <param name="componentProductCode">Code of the component product.</param>
    /// <param name="componentProductName">Name of the component product.</param>
    /// <param name="componentQuantity">Quantity of component required per finished unit.</param>
    /// <param name="componentUnitCode">Unit of measure for the component.</param>
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

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IdResponse>())!.Id;
    }

    /// <summary>
    /// Creates a production order and returns its id.
    /// </summary>
    /// <param name="billOfMaterialsId">Id of the bill of materials to produce against.</param>
    /// <param name="workCenterId">Id of the work center to assign.</param>
    /// <param name="plannedQuantity">Planned quantity to produce.</param>
    /// <param name="unitCode">Unit of measure for the finished product.</param>
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

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IdResponse>())!.Id;
    }

    /// <summary>
    /// Confirms a production order, assigning the component to the given warehouse for sourcing.
    /// </summary>
    /// <param name="productionOrderId">Id of the production order to confirm.</param>
    /// <param name="componentProductId">Id of the component product to assign.</param>
    /// <param name="warehouseId">Id of the warehouse the component will be sourced from.</param>
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

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"ConfirmProductionOrder {(int)response.StatusCode}: {body}");
        }
    }

    /// <summary>
    /// Starts a production order.
    /// </summary>
    /// <param name="productionOrderId">Id of the production order to start.</param>
    public async Task StartProductionOrderAsync(Guid productionOrderId)
    {
        var response = await _client.PutAsync($"/production/orders/{productionOrderId}/start", null);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Completes a production order, consuming components and delivering finished goods to the given warehouse.
    /// </summary>
    /// <param name="productionOrderId">Id of the production order to complete.</param>
    /// <param name="finishedGoodsWarehouseId">Id of the warehouse to deliver finished goods to.</param>
    /// <param name="actualQuantity">Actual quantity produced.</param>
    /// <param name="componentProductId">Id of the component product consumed.</param>
    /// <param name="componentWarehouseId">Id of the warehouse the component was consumed from.</param>
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

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"CompleteProductionOrder {(int)response.StatusCode}: {body}");
        }
    }

    // Sales

    /// <summary>
    /// Creates a customer and returns its id.
    /// </summary>
    /// <param name="code">Unique customer code.</param>
    /// <param name="name">Display name of the customer.</param>
    /// <param name="contactEmail">Primary contact email address.</param>
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

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IdResponse>())!.Id;
    }

    /// <summary>
    /// Creates a sales order with a single line and returns its id.
    /// </summary>
    /// <param name="customerId">Id of the customer.</param>
    /// <param name="productId">Id of the product to order.</param>
    /// <param name="productCode">Product code.</param>
    /// <param name="productName">Product display name.</param>
    /// <param name="quantity">Quantity ordered.</param>
    /// <param name="unitCode">Unit of measure code.</param>
    /// <param name="unitPrice">Price per unit.</param>
    /// <param name="currencyCode">Currency code, defaults to EUR.</param>
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

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IdResponse>())!.Id;
    }

    /// <summary>
    /// Confirms a sales order, reserving stock from the given warehouse.
    /// </summary>
    /// <param name="salesOrderId">Id of the sales order to confirm.</param>
    /// <param name="warehouseId">Id of the warehouse to reserve stock from.</param>
    public async Task ConfirmSalesOrderAsync(Guid salesOrderId, Guid warehouseId)
    {
        var response = await _client.PutAsJsonAsync($"/sales/orders/{salesOrderId}/confirm", new
        {
            salesOrderId,
            warehouseId,
        });

        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Ships a sales order line and returns the shipment id.
    /// </summary>
    /// <param name="salesOrderId">Id of the sales order to ship.</param>
    /// <param name="salesOrderLineId">Id of the sales order line to ship.</param>
    /// <param name="productId">Id of the product being shipped.</param>
    /// <param name="productCode">Product code.</param>
    /// <param name="warehouseId">Id of the warehouse to ship from.</param>
    /// <param name="quantity">Quantity to ship.</param>
    /// <param name="unitCode">Unit of measure code.</param>
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

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IdResponse>())!.Id;
    }

    private sealed record IdResponse(Guid Id);
}