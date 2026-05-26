using CoreGearERP.Inventory.gRPC;
using CoreGearERP.Inventory.Application.Contracts;
using Grpc.Core;

namespace CoreGearERP.Inventory.Infrastructure.gRPC;

/// <summary>
/// gRPC server implementation for inventory query operations.
/// Delegates to the existing InventoryQueryService.
/// </summary>
public class InventoryQueryGrpcService : InventoryQueries.InventoryQueriesBase
{
    private readonly InventoryQueryService _queryService;

    /// <summary>
    /// Constructor with dependencies injected.
    /// </summary>
    /// <param name="queryService">InventoryQueryService handles the business logic for inventory queries.</param>
    public InventoryQueryGrpcService(InventoryQueryService queryService)
    {
        _queryService = queryService;
    }

    public override async Task<GetAvailableStockResponse> GetAvailableStock(
        GetAvailableStockRequest request,
        ServerCallContext context)
    {
        var quantity = await _queryService.GetAvailableStockAsync(
            productId:         Guid.Parse(request.ProductId),
            tenantId:          Guid.Parse(request.TenantId),
            cancellationToken: context.CancellationToken);

        return new GetAvailableStockResponse { Quantity = (double)quantity };
    }

    public override async Task<GetAvailableStockResponse> GetAvailableStockInWarehouse(
        GetStockInWarehouseRequest request,
        ServerCallContext context)
    {
        var quantity = await _queryService.GetAvailableStockInWarehouseAsync(
            productId:         Guid.Parse(request.ProductId),
            warehouseId:       Guid.Parse(request.WarehouseId),
            tenantId:          Guid.Parse(request.TenantId),
            cancellationToken: context.CancellationToken);

        return new GetAvailableStockResponse { Quantity = (double)quantity };
    }

    public override async Task<FindBestWarehouseResponse> FindBestWarehouse(
        FindBestWarehouseRequest request,
        ServerCallContext context)
    {
        var warehouseId = await _queryService.FindBestWarehouseAsync(
            productId:         Guid.Parse(request.ProductId),
            quantity:          (decimal)request.Quantity,
            tenantId:          Guid.Parse(request.TenantId),
            cancellationToken: context.CancellationToken);

        return new FindBestWarehouseResponse { WarehouseId = warehouseId.ToString() };
    }
}