using CoreGearERP.Common.Application.Interfaces;

namespace CoreGearERP.Sales.Application.Shipments;

/// <summary>
/// Command to ship goods against a confirmed sales order.
/// Releases stock reservations and creates SalesShipment stock movements.
/// Cross-module stock operations are in-process now, replaced with gRPC at M4.
/// </summary>
public record ShipOrderCommand(
    Guid SalesOrderId,
    Guid WarehouseId,
    string Notes,
    IReadOnlyList<ShipOrderLineDto> Lines) : ICommand<Guid>;

/// <summary>
/// Line item input for shipment creation.
/// </summary>
public record ShipOrderLineDto(
    Guid SalesOrderLineId,
    Guid ProductId,
    string ProductCode,
    decimal Quantity,
    string UnitCode);