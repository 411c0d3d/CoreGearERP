using CoreGearERP.Common.Application.Interfaces;

namespace CoreGearERP.Production.Application.ProductionOrders.StartProductionOrder;

/// <summary>
/// Command to start a Confirmed production order.
/// </summary>
public record StartProductionOrderCommand(Guid ProductionOrderId) : ICommand<Unit>;