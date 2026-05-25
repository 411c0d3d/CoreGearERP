using CoreGearERP.Common.Application.Interfaces;

namespace CoreGearERP.Procurement.Application.PurchaseOrders;

/// <summary>
/// Command to receive goods against a purchase order line.
/// </summary>
public record ReceiveGoodsCommand(
    Guid PurchaseOrderId,
    Guid PurchaseOrderLineId,
    Guid WarehouseId,
    decimal Quantity) : ICommand<Unit>;