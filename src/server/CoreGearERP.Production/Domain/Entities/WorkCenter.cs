using CoreGearERP.Common.Domain.Entities;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Production.Domain.Enums;

namespace CoreGearERP.Production.Domain.Entities;

/// <summary>
/// Represents a physical location where production work is performed.
/// </summary>
public class WorkCenter : BaseEntity
{
    /// <summary>
    /// Human-facing work center code. Unique per tenant.
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Maximum units that can be produced per hour.
    /// </summary>
    public decimal CapacityPerHour { get; private set; }

    private WorkCenter() { }

    /// <summary>
    /// Factory method. The only way to create a valid WorkCenter.
    /// </summary>
    public static WorkCenter Create(
        string code,
        string name,
        decimal capacityPerHour,
        Guid tenantId,
        Guid createdBy,
        string description = "")
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException("Work center code cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Work center name cannot be empty.");
        }

        if (capacityPerHour <= 0)
        {
            throw new DomainException("Work center capacity must be greater than zero.");
        }

        var workCenter = new WorkCenter
        {
            Code             = code.Trim().ToUpperInvariant(),
            Name             = name.Trim(),
            Description      = description.Trim(),
            CapacityPerHour  = capacityPerHour
        };

        workCenter.Status = WorkCenterStatus.Active.ToString();
        workCenter.SetCreated(tenantId, createdBy);

        return workCenter;
    }

    /// <summary>
    /// Updates work center details.
    /// </summary>
    public void Update(string name, decimal capacityPerHour, string description, Guid modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Work center name cannot be empty.");
        }

        if (capacityPerHour <= 0)
        {
            throw new DomainException("Work center capacity must be greater than zero.");
        }

        Name            = name.Trim();
        Description     = description.Trim();
        CapacityPerHour = capacityPerHour;
        SetModified(modifiedBy);
    }

    /// <summary>
    /// Deactivates the work center.
    /// </summary>
    public void Deactivate(Guid modifiedBy)
    {
        if (Status == WorkCenterStatus.Inactive.ToString())
        {
            throw new DomainException("Work center is already inactive.");
        }

        Status = WorkCenterStatus.Inactive.ToString();
        SetModified(modifiedBy);
    }

    /// <summary>
    /// Activates the work center.
    /// </summary>
    public void Activate(Guid modifiedBy)
    {
        if (Status == WorkCenterStatus.Active.ToString())
        {
            throw new DomainException("Work center is already active.");
        }

        Status = WorkCenterStatus.Active.ToString();
        SetModified(modifiedBy);
    }
}