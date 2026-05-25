using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Production.Application.ProductionOrders.ConfirmProductionOrder;

namespace CoreGearERP.Production.Application.ProductionOrders.CompleteProductionOrder;

/// <summary>
/// Command to complete an InProgress production order.
/// Caller specifies which warehouse each component is consumed from.
/// </summary>
public record CompleteProductionOrderCommand(
    Guid ProductionOrderId,
    Guid FinishedGoodsWarehouseId,
    decimal ActualQuantity,
    IReadOnlyList<ComponentWarehouseAssignment> ComponentWarehouses) : ICommand<Unit>;
