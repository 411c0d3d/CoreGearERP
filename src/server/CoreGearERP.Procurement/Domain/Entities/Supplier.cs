using CoreGearERP.Common.Domain.Entities;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Procurement.Domain.Enums;

namespace CoreGearERP.Procurement.Domain.Entities;

/// <summary>
/// Represents a supplier that goods are procured from.
/// </summary>
public class Supplier : BaseEntity
{
    /// <summary>
    /// Human-facing supplier code. Unique per tenant.
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string ContactEmail { get; private set; } = string.Empty;

    public string ContactPhone { get; private set; } = string.Empty;

    public string Address { get; private set; } = string.Empty;

    private Supplier()
    {
    }

    /// <summary>
    /// Factory method. The only way to create a valid Supplier.
    /// </summary>
    public static Supplier Create(
        string code,
        string name,
        string contactEmail,
        string contactPhone,
        string address,
        Guid tenantId,
        Guid createdBy)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException("Supplier code cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Supplier name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(contactEmail))
        {
            throw new DomainException("Supplier contact email cannot be empty.");
        }

        var supplier = new Supplier
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            ContactEmail = contactEmail.Trim().ToLowerInvariant(),
            ContactPhone = contactPhone.Trim(),
            Address = address.Trim()
        };

        supplier.Status = SupplierStatus.Active.ToString();
        supplier.SetCreated(tenantId, createdBy);

        return supplier;
    }

    /// <summary>
    /// Updates supplier contact details.
    /// </summary>
    public void Update(
        string name,
        string contactEmail,
        string contactPhone,
        string address,
        Guid modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Supplier name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(contactEmail))
        {
            throw new DomainException("Supplier contact email cannot be empty.");
        }

        Name = name.Trim();
        ContactEmail = contactEmail.Trim().ToLowerInvariant();
        ContactPhone = contactPhone.Trim();
        Address = address.Trim();
        SetModified(modifiedBy);
    }

    /// <summary>
    /// Deactivates the supplier. Inactive suppliers cannot have new POs raised against them.
    /// </summary>
    public void Deactivate(Guid modifiedBy)
    {
        if (Status == SupplierStatus.Inactive.ToString())
        {
            throw new DomainException("Supplier is already inactive.");
        }

        Status = SupplierStatus.Inactive.ToString();
        SetModified(modifiedBy);
    }

    /// <summary>
    /// Activates the supplier.
    /// </summary>
    public void Activate(Guid modifiedBy)
    {
        if (Status == SupplierStatus.Active.ToString())
        {
            throw new DomainException("Supplier is already active.");
        }

        Status = SupplierStatus.Active.ToString();
        SetModified(modifiedBy);
    }
}