using CoreGearERP.Common.Application.Interfaces;

namespace CoreGearERP.Procurement.Application.PurchaseOrders;

/// <summary>
/// Command to confirm a Draft purchase order.
/// </summary>
public record ConfirmPurchaseOrderCommand(Guid PurchaseOrderId) : ICommand<Unit>;