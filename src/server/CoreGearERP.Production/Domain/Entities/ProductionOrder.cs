using CoreGearERP.Common.Domain.Entities;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Common.Domain.ValueObjects;
using CoreGearERP.Production.Domain.Enums;

namespace CoreGearERP.Production.Domain.Entities;

/// <summary>
/// Represents a production run against a BillOfMaterials.
/// Tracks planned vs actual component consumption.
/// Completed production orders are immutable.
/// </summary>
public class ProductionOrder : BaseEntity
{
    /// <summary>
    /// Human-facing order number. Unique per tenant.
    /// </summary>
    public string OrderNumber { get; private set; } = string.Empty;

    public Guid BillOfMaterialsId { get; private set; }

    public string FinishedProductCode { get; private set; } = string.Empty;

    public string FinishedProductName { get; private set; } = string.Empty;

    public Guid WorkCenterId { get; private set; }

    public string WorkCenterCode { get; private set; } = string.Empty;

    /// <summary>
    /// Planned quantity to produce.
    /// </summary>
    public Quantity PlannedQuantity { get; private set; } = null!;

    /// <summary>
    /// Actual quantity produced. Set on completion.
    /// </summary>
    public Quantity? ActualQuantity { get; private set; }

    public string Notes { get; private set; } = string.Empty;

    private ProductionOrder()
    {
    }

    /// <summary>
    /// Factory method. Creates a ProductionOrder in Draft status.
    /// </summary>
    public static ProductionOrder Create(
        string orderNumber,
        Guid billOfMaterialsId,
        string finishedProductCode,
        string finishedProductName,
        Guid workCenterId,
        string workCenterCode,
        Quantity plannedQuantity,
        string notes,
        Guid tenantId,
        Guid createdBy)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            throw new DomainException("Production order number cannot be empty.");
        }

        if (plannedQuantity.Value <= 0)
        {
            throw new DomainException("Planned quantity must be greater than zero.");
        }

        var order = new ProductionOrder
        {
            OrderNumber = orderNumber.Trim().ToUpperInvariant(),
            BillOfMaterialsId = billOfMaterialsId,
            FinishedProductCode = finishedProductCode.Trim().ToUpperInvariant(),
            FinishedProductName = finishedProductName.Trim(),
            WorkCenterId = workCenterId,
            WorkCenterCode = workCenterCode.Trim().ToUpperInvariant(),
            PlannedQuantity = plannedQuantity,
            Notes = notes.Trim()
        };

        order.Status = ProductionOrderStatus.Draft.ToString();
        order.SetCreated(tenantId, createdBy);

        return order;
    }

    /// <summary>
    /// Confirms the production order. Stock reservations are made before calling this.
    /// </summary>
    public void Confirm(Guid modifiedBy)
    {
        if (Status != ProductionOrderStatus.Draft.ToString())
        {
            throw new DomainException("Only a Draft production order can be confirmed.");
        }

        Status = ProductionOrderStatus.Confirmed.ToString();
        ConfirmedAt = DateTime.UtcNow;
        SetModified(modifiedBy);
    }

    /// <summary>
    /// Starts the production order.
    /// </summary>
    public void Start(Guid modifiedBy)
    {
        if (Status != ProductionOrderStatus.Confirmed.ToString())
        {
            throw new DomainException("Only a Confirmed production order can be started.");
        }

        Status = ProductionOrderStatus.InProgress.ToString();
        SetModified(modifiedBy);
    }

    /// <summary>
    /// Completes the production order with actual quantity produced.
    /// Completed orders are immutable -- no further changes allowed.
    /// </summary>
    public void Complete(Quantity actualQuantity, Guid modifiedBy)
    {
        if (Status != ProductionOrderStatus.InProgress.ToString())
        {
            throw new DomainException("Only an InProgress production order can be completed.");
        }

        if (actualQuantity.Value <= 0)
        {
            throw new DomainException("Actual quantity must be greater than zero.");
        }

        ActualQuantity = actualQuantity;
        Status = ProductionOrderStatus.Completed.ToString();
        CompletedAt = DateTime.UtcNow;
        SetModified(modifiedBy);
    }

    /// <summary>
    /// Cancels the production order. Cannot cancel once InProgress or Completed.
    /// </summary>
    public void Cancel(Guid modifiedBy)
    {
        if (Status == ProductionOrderStatus.InProgress.ToString() ||
            Status == ProductionOrderStatus.Completed.ToString())
        {
            throw new DomainException("Cannot cancel a production order that is InProgress or Completed.");
        }

        if (Status == ProductionOrderStatus.Cancelled.ToString())
        {
            throw new DomainException("Production order is already cancelled.");
        }

        Status = ProductionOrderStatus.Cancelled.ToString();
        CancelledAt = DateTime.UtcNow;
        SetModified(modifiedBy);
    }
}