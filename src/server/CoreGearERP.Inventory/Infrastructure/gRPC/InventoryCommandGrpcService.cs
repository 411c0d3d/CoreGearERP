using CoreGearERP.Inventory.gRPC;
using CoreGearERP.Inventory.Application.Contracts;
using Grpc.Core;

namespace CoreGearERP.Inventory.Infrastructure.gRPC;

/// <summary>
/// gRPC server implementation for inventory command operations.
/// Delegates to the existing InventoryCommandService.
/// </summary>
public class InventoryCommandGrpcService : InventoryCommands.InventoryCommandsBase
{
    private readonly InventoryCommandService _commandService;

    /// <summary>
    /// Constructor with dependencies injected.
    /// </summary>
    /// <param name="commandService">InventoryCommandService is used to handle the business logic for inventory commands.</param>
    public InventoryCommandGrpcService(InventoryCommandService commandService)
    {
        _commandService = commandService;
    }

    public override async Task<EmptyResponse> AddStock(AddStockRequest request, ServerCallContext context)
    {
        await _commandService.AddStockAsync(
            productId:       Guid.Parse(request.ProductId),
            warehouseId:     Guid.Parse(request.WarehouseId),
            quantity:        (decimal)request.Quantity,
            unitCode:        request.UnitCode,
            referenceId:     Guid.Parse(request.ReferenceId),
            referenceNumber: request.ReferenceNumber,
            tenantId:        Guid.Parse(request.TenantId),
            userId:          Guid.Parse(request.UserId),
            cancellationToken: context.CancellationToken);

        return new EmptyResponse();
    }

    public override async Task<EmptyResponse> AddStockFromProduction(AddStockRequest request, ServerCallContext context)
    {
        await _commandService.AddStockFromProductionAsync(
            productId:       Guid.Parse(request.ProductId),
            warehouseId:     Guid.Parse(request.WarehouseId),
            quantity:        (decimal)request.Quantity,
            unitCode:        request.UnitCode,
            referenceId:     Guid.Parse(request.ReferenceId),
            referenceNumber: request.ReferenceNumber,
            tenantId:        Guid.Parse(request.TenantId),
            userId:          Guid.Parse(request.UserId),
            cancellationToken: context.CancellationToken);

        return new EmptyResponse();
    }

    public override async Task<EmptyResponse> IssueStock(IssueStockRequest request, ServerCallContext context)
    {
        await _commandService.IssueStockAsync(
            productId:       Guid.Parse(request.ProductId),
            warehouseId:     Guid.Parse(request.WarehouseId),
            quantity:        (decimal)request.Quantity,
            unitCode:        request.UnitCode,
            referenceId:     Guid.Parse(request.ReferenceId),
            referenceNumber: request.ReferenceNumber,
            tenantId:        Guid.Parse(request.TenantId),
            userId:          Guid.Parse(request.UserId),
            cancellationToken: context.CancellationToken);

        return new EmptyResponse();
    }

    public override async Task<EmptyResponse> ReserveStockInWarehouse(ReserveStockRequest request, ServerCallContext context)
    {
        await _commandService.ReserveStockInWarehouseAsync(
            productId:   Guid.Parse(request.ProductId),
            warehouseId: Guid.Parse(request.WarehouseId),
            quantity:    (decimal)request.Quantity,
            unitCode:    request.UnitCode,
            tenantId:    Guid.Parse(request.TenantId),
            userId:      Guid.Parse(request.UserId),
            cancellationToken: context.CancellationToken);

        return new EmptyResponse();
    }

    public override async Task<EmptyResponse> ReleaseReservation(ReserveStockRequest request, ServerCallContext context)
    {
        await _commandService.ReleaseReservationAsync(
            productId:   Guid.Parse(request.ProductId),
            warehouseId: Guid.Parse(request.WarehouseId),
            quantity:    (decimal)request.Quantity,
            unitCode:    request.UnitCode,
            tenantId:    Guid.Parse(request.TenantId),
            userId:      Guid.Parse(request.UserId),
            cancellationToken: context.CancellationToken);

        return new EmptyResponse();
    }

    public override async Task<EmptyResponse> ShipStock(AddStockRequest request, ServerCallContext context)
    {
        await _commandService.ShipStockAsync(
            productId:       Guid.Parse(request.ProductId),
            warehouseId:     Guid.Parse(request.WarehouseId),
            quantity:        (decimal)request.Quantity,
            unitCode:        request.UnitCode,
            referenceId:     Guid.Parse(request.ReferenceId),
            referenceNumber: request.ReferenceNumber,
            tenantId:        Guid.Parse(request.TenantId),
            userId:          Guid.Parse(request.UserId),
            cancellationToken: context.CancellationToken);

        return new EmptyResponse();
    }
}