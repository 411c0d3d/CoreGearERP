using CoreGearERP.Common.Application.Interfaces;

namespace CoreGearERP.Sales.Application.SalesOrders.ConfirmSalesOrder;

/// <summary>
/// Command to confirm a Draft sales order.
/// Checks stock availability and reserves stock per line before confirming.
/// Cross-module stock check is in-process now, replaced with gRPC at M4.
/// </summary>
public record ConfirmSalesOrderCommand(
    Guid SalesOrderId,
    Guid? WarehouseId = null) : ICommand<Unit>;