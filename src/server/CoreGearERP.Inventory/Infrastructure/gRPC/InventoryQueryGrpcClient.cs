using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Inventory.gRPC;

namespace CoreGearERP.Inventory.Infrastructure.gRPC;

/// <summary>
/// gRPC client implementation of IInventoryQueryService.
/// Replaces the in-process InventoryQueryService at M4.
/// </summary>
public class InventoryQueryGrpcClient : IInventoryQueryService
{
    private readonly InventoryQueries.InventoryQueriesClient _client;

    /// <summary>
    /// Constructor with dependencies injected.
    /// </summary>
    /// <param name="client">The gRPC client for inventory queries, injected by DI.</param>
    public InventoryQueryGrpcClient(InventoryQueries.InventoryQueriesClient client)
    {
        _client = client;
    }

    public async Task<decimal> GetAvailableStockAsync(
        Guid productId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAvailableStockInWarehouseAsync(
            new GetStockInWarehouseRequest
            {
                ProductId   = productId.ToString(),
                WarehouseId = Guid.Empty.ToString(),
                TenantId    = tenantId.ToString()
            }, cancellationToken: cancellationToken);

        return (decimal)response.Quantity;
    }

    public async Task<decimal> GetAvailableStockInWarehouseAsync(
        Guid productId, Guid warehouseId, Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAvailableStockInWarehouseAsync(new GetStockInWarehouseRequest
        {
            ProductId   = productId.ToString(),
            WarehouseId = warehouseId.ToString(),
            TenantId    = tenantId.ToString()
        }, cancellationToken: cancellationToken);

        return (decimal)response.Quantity;
    }

    public async Task<Guid> FindBestWarehouseAsync(
        Guid productId, decimal quantity, Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var response = await _client.FindBestWarehouseAsync(new FindBestWarehouseRequest
        {
            ProductId = productId.ToString(),
            Quantity  = (double)quantity,
            TenantId  = tenantId.ToString()
        }, cancellationToken: cancellationToken);

        return string.IsNullOrEmpty(response.WarehouseId)
            ? Guid.Empty
            : Guid.Parse(response.WarehouseId);
    }
}