using CoreGearERP.Common.Application.Interfaces;

namespace CoreGearERP.Production.Application.ProductionOrders.CreateProductionOrder;

/// <summary>
/// Command to create a ProductionOrder in Draft status.
/// </summary>
public record CreateProductionOrderCommand(
    Guid BillOfMaterialsId,
    Guid WorkCenterId,
    decimal PlannedQuantity,
    string UnitCode,
    string Notes = "") : ICommand<Guid>;