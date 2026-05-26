using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Inventory.gRPC;

namespace CoreGearERP.Inventory.Infrastructure.gRPC;

/// <summary>
/// gRPC client implementation of IInventoryCommandService.
/// Replaces the in-process InventoryCommandService at M4.
/// </summary>
public class InventoryCommandGrpcClient : IInventoryCommandService
{
    private readonly InventoryCommands.InventoryCommandsClient _client;

    /// <summary>
    /// Constructor with dependencies injected.
    /// </summary>
    /// <param name="client">The gRPC client for inventory commands, injected by DI.</param>
    public InventoryCommandGrpcClient(InventoryCommands.InventoryCommandsClient client)
    {
        _client = client;
    }

    public async Task AddStockAsync(
        Guid productId, Guid warehouseId, decimal quantity, string unitCode,
        Guid referenceId, string referenceNumber, Guid tenantId, Guid userId,
        CancellationToken cancellationToken = default)
    {
        await _client.AddStockAsync(new AddStockRequest
        {
            ProductId       = productId.ToString(),
            WarehouseId     = warehouseId.ToString(),
            Quantity        = (double)quantity,
            UnitCode        = unitCode,
            ReferenceId     = referenceId.ToString(),
            ReferenceNumber = referenceNumber,
            TenantId        = tenantId.ToString(),
            UserId          = userId.ToString()
        }, cancellationToken: cancellationToken);
    }

    public async Task AddStockFromProductionAsync(
        Guid productId, Guid warehouseId, decimal quantity, string unitCode,
        Guid referenceId, string referenceNumber, Guid tenantId, Guid userId,
        CancellationToken cancellationToken = default)
    {
        await _client.AddStockFromProductionAsync(new AddStockRequest
        {
            ProductId       = productId.ToString(),
            WarehouseId     = warehouseId.ToString(),
            Quantity        = (double)quantity,
            UnitCode        = unitCode,
            ReferenceId     = referenceId.ToString(),
            ReferenceNumber = referenceNumber,
            TenantId        = tenantId.ToString(),
            UserId          = userId.ToString()
        }, cancellationToken: cancellationToken);
    }

    public async Task IssueStockAsync(
        Guid productId, Guid warehouseId, decimal quantity, string unitCode,
        Guid referenceId, string referenceNumber, Guid tenantId, Guid userId,
        CancellationToken cancellationToken = default)
    {
        await _client.IssueStockAsync(new IssueStockRequest
        {
            ProductId       = productId.ToString(),
            WarehouseId     = warehouseId.ToString(),
            Quantity        = (double)quantity,
            UnitCode        = unitCode,
            ReferenceId     = referenceId.ToString(),
            ReferenceNumber = referenceNumber,
            TenantId        = tenantId.ToString(),
            UserId          = userId.ToString()
        }, cancellationToken: cancellationToken);
    }

    public async Task ReserveStockInWarehouseAsync(
        Guid productId, Guid warehouseId, decimal quantity, string unitCode,
        Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        await _client.ReserveStockInWarehouseAsync(new ReserveStockRequest
        {
            ProductId   = productId.ToString(),
            WarehouseId = warehouseId.ToString(),
            Quantity    = (double)quantity,
            UnitCode    = unitCode,
            TenantId    = tenantId.ToString(),
            UserId      = userId.ToString()
        }, cancellationToken: cancellationToken);
    }

    public async Task ReleaseReservationAsync(
        Guid productId, Guid warehouseId, decimal quantity, string unitCode,
        Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        await _client.ReleaseReservationAsync(new ReserveStockRequest
        {
            ProductId   = productId.ToString(),
            WarehouseId = warehouseId.ToString(),
            Quantity    = (double)quantity,
            UnitCode    = unitCode,
            TenantId    = tenantId.ToString(),
            UserId      = userId.ToString()
        }, cancellationToken: cancellationToken);
    }

    public async Task ShipStockAsync(
        Guid productId, Guid warehouseId, decimal quantity, string unitCode,
        Guid referenceId, string referenceNumber, Guid tenantId, Guid userId,
        CancellationToken cancellationToken = default)
    {
        await _client.ShipStockAsync(new AddStockRequest
        {
            ProductId       = productId.ToString(),
            WarehouseId     = warehouseId.ToString(),
            Quantity        = (double)quantity,
            UnitCode        = unitCode,
            ReferenceId     = referenceId.ToString(),
            ReferenceNumber = referenceNumber,
            TenantId        = tenantId.ToString(),
            UserId          = userId.ToString()
        }, cancellationToken: cancellationToken);
    }
}