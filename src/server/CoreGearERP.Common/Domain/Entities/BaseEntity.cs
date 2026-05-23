namespace CoreGearERP.Common.Domain.Entities;

/// <summary>
/// Base class for all domain entities across all modules.
/// Every table in the database maps to a class that inherits from this.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Surrogate primary key. Always a UUID, never an int identity.
    /// Business keys like OrderNumber are separate columns.
    /// </summary>
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Every entity belongs to a tenant. This is the foundation of data isolation.
    /// EF Core global query filters use this to ensure tenants never see each other's data.
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// Soft delete flag. Nothing in this system is ever hard deleted.
    /// EF Core global query filters exclude IsDeleted = true from all queries automatically.
    /// </summary>
    public bool IsDeleted { get; private set; }

    /// <summary>
    /// Status of the entity. Always a string, never a set of bool flags.
    /// Concrete entities define what values are valid via their own enum or constants.
    /// </summary>
    public string Status { get; protected set; } = string.Empty;

    // Audit columns
    // These are non-negotiable. Every state change is attributed to a user and timestamped.
    // Adding these after data exists means a painful migration touching every table.

    public DateTime CreatedAt { get; private set; }

    public Guid CreatedBy { get; private set; }

    public DateTime ModifiedAt { get; private set; }

    public Guid ModifiedBy { get; private set; }

    // State transition timestamps
    // Nullable because they are only set when the transition actually happens.
    // Cheaper to add now than to reconstruct from audit logs later.
    public DateTime? ConfirmedAt { get; protected set; }

    public DateTime? CompletedAt { get; protected set; }

    public DateTime? CancelledAt { get; protected set; }

    /// <summary>
    /// Sets the initial audit state when an entity is first created.
    /// Called in the factory method or constructor of each concrete entity.
    /// </summary>
    protected void SetCreated(Guid tenantId, Guid createdBy)
    {
        TenantId = tenantId;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
        ModifiedAt = DateTime.UtcNow;
        ModifiedBy = createdBy;
    }

    /// <summary>
    /// Updates the audit state on every modification.
    /// Called inside domain methods that mutate state, not from outside the entity.
    /// </summary>
    protected void SetModified(Guid modifiedBy)
    {
        ModifiedAt = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
    }

    /// <summary>
    /// Marks the entity as soft deleted.
    /// Never called directly -- goes through a domain method on the concrete entity.
    /// </summary>
    protected void SetDeleted(Guid deletedBy)
    {
        IsDeleted = true;
        SetModified(deletedBy);
    }
}