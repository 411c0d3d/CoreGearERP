namespace CoreGearERP.Common.Application.Events;

/// <summary>Raised by Sales when a shipment is marked as shipped.</summary>
/// <param name="ShipmentId">The Id of the shipment that was shipped.</param>
/// <param name="ShipmentNumber">Human-facing shipment number.</param>
/// <param name="SalesOrderId">The sales order this shipment belongs to.</param>
/// <param name="SalesOrderNumber">Human-facing sales order number.</param>
/// <param name="TotalAmount">Pre-calculated shipment total across all lines: sum of UnitPrice * QuantityShipped.</param>
/// <param name="CurrencyCode">ISO 4217 currency code. Taken from the first line -- all lines on one order share currency.</param>
/// <param name="TenantId">Tenant this event belongs to.</param>
/// <param name="OccurredAt">UTC timestamp when the shipment was marked shipped.</param>
public record SalesOrderShippedEvent(
    Guid ShipmentId,
    string ShipmentNumber,
    Guid SalesOrderId,
    string SalesOrderNumber,
    decimal TotalAmount,
    string CurrencyCode,
    Guid TenantId,
    DateTime OccurredAt);