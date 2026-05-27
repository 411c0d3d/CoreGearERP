using CoreGearERP.Common.Domain.Entities;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Common.Domain.ValueObjects;
using CoreGearERP.Finance.Domain.Enums;

namespace CoreGearERP.Finance.Domain.Entities;

/// <summary>
/// Represents an immutable cost posting against a financial period.
/// Created once, never updated. Corrections are made via reversal entries.
/// Production orders post with Amount = 0 pending costing reconciliation.
/// </summary>
public class CostEntry : BaseEntity
{
    public Guid PeriodId { get; private set; }

    /// <summary>
    /// Id of the source document (GoodsReceipt, ProductionOrder, Shipment).
    /// </summary>
    public Guid SourceDocumentId { get; private set; }

    /// <summary>
    /// Human-facing reference number of the source document.
    /// </summary>
    public string SourceDocumentNumber { get; private set; } = string.Empty;

    public CostEntrySourceType SourceType { get; private set; }

    public Money Amount { get; private set; } = null!;

    /// <summary>
    /// True when this entry reverses a previous cost entry.
    /// </summary>
    public bool IsReversal { get; private set; }

    /// <summary>
    /// Id of the original entry being reversed. Null for normal entries.
    /// </summary>
    public Guid? ReversedCostEntryId { get; private set; }

    /// <summary>
    /// True when Amount is zero because production costing is pending reconciliation.
    /// </summary>
    public bool IsPendingCosting { get; private set; }

    private CostEntry() { }

    /// <summary>
    /// Factory method. Creates a normal cost entry.
    /// </summary>
    public static CostEntry Create(
        Guid periodId,
        Guid sourceDocumentId,
        string sourceDocumentNumber,
        CostEntrySourceType sourceType,
        Money amount,
        bool isPendingCosting,
        Guid tenantId,
        Guid createdBy)
    {
        if (string.IsNullOrWhiteSpace(sourceDocumentNumber))
        {
            throw new DomainException("Source document number cannot be empty.");
        }

        var entry = new CostEntry
        {
            PeriodId             = periodId,
            SourceDocumentId     = sourceDocumentId,
            SourceDocumentNumber = sourceDocumentNumber.Trim().ToUpperInvariant(),
            SourceType           = sourceType,
            Amount               = amount,
            IsReversal           = false,
            ReversedCostEntryId  = null,
            IsPendingCosting     = isPendingCosting
        };

        entry.Status = "Posted";
        entry.SetCreated(tenantId, createdBy);

        return entry;
    }

    /// <summary>
    /// Factory method. Creates a reversal entry that negates a previous cost entry.
    /// </summary>
    public static CostEntry CreateReversal(
        Guid periodId,
        CostEntry original,
        Guid tenantId,
        Guid createdBy)
    {
        var reversal = new CostEntry
        {
            PeriodId             = periodId,
            SourceDocumentId     = original.SourceDocumentId,
            SourceDocumentNumber = original.SourceDocumentNumber,
            SourceType           = original.SourceType,
            Amount               = original.Amount.Negate(),
            IsReversal           = true,
            ReversedCostEntryId  = original.Id,
            IsPendingCosting     = false
        };

        reversal.Status = "Posted";
        reversal.SetCreated(tenantId, createdBy);

        return reversal;
    }
}