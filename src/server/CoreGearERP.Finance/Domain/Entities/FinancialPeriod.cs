using CoreGearERP.Common.Domain.Entities;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Finance.Domain.Enums;

namespace CoreGearERP.Finance.Domain.Entities;

/// <summary>
/// Represents a financial period (typically one calendar month).
/// Cost entries can only be posted into an open period.
/// A closed period is immutable -- no further postings permitted.
/// </summary>
public class FinancialPeriod : BaseEntity
{
    /// <summary>
    /// Human-facing period name, e.g. "2026-05".
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    public DateTime StartDate { get; private set; }

    public DateTime EndDate { get; private set; }

    private FinancialPeriod() { }

    /// <summary>
    /// Factory method. Creates a period in Open status.
    /// </summary>
    public static FinancialPeriod Create(
        string name,
        DateTime startDate,
        DateTime endDate,
        Guid tenantId,
        Guid createdBy)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Financial period name cannot be empty.");
        }

        if (endDate <= startDate)
        {
            throw new DomainException("Period end date must be after start date.");
        }

        var period = new FinancialPeriod
        {
            Name      = name.Trim(),
            StartDate = startDate,
            EndDate   = endDate
        };

        period.Status = FinancialPeriodStatus.Open.ToString();
        period.SetCreated(tenantId, createdBy);

        return period;
    }

    /// <summary>
    /// Closes the period. No further cost entries permitted after this.
    /// </summary>
    public void Close(Guid modifiedBy)
    {
        if (Status == FinancialPeriodStatus.Closed.ToString())
        {
            throw new DomainException("Financial period is already closed.");
        }

        Status      = FinancialPeriodStatus.Closed.ToString();
        CompletedAt = DateTime.UtcNow;
        SetModified(modifiedBy);
    }

    /// <summary>
    /// Returns true if this period accepts postings for the given date.
    /// </summary>
    public bool AcceptsPostingFor(DateTime date)
    {
        return Status == FinancialPeriodStatus.Open.ToString()
            && date >= StartDate
            && date <= EndDate;
    }
}