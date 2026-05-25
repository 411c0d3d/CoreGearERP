using CoreGearERP.Common.Application.Interfaces;

namespace CoreGearERP.Production.Application.ProductionOrders.ConfirmProductionOrder;

/// <summary>
/// Command to confirm a Draft production order.
/// Caller specifies which warehouse each component is sourced from.
/// Stock availability is checked and reserved per component per warehouse.
/// </summary>
public record ConfirmProductionOrderCommand(
    Guid ProductionOrderId,
    IReadOnlyList<ComponentWarehouseAssignment> ComponentWarehouses) : ICommand<Unit>;

/// <summary>
/// Maps a component product to the warehouse it will be sourced from.
/// </summary>
public record ComponentWarehouseAssignment(
    Guid ComponentProductId,
    Guid WarehouseId);