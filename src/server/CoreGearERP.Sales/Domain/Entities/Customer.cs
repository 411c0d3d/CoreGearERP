using CoreGearERP.Common.Domain.Entities;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Sales.Domain.Enums;

namespace CoreGearERP.Sales.Domain.Entities;

/// <summary>
/// Represents a customer that sales orders are raised against.
/// </summary>
public class Customer : BaseEntity
{
    /// <summary>
    /// Human-facing customer code. Unique per tenant.
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string ContactEmail { get; private set; } = string.Empty;

    public string ContactPhone { get; private set; } = string.Empty;

    public string Address { get; private set; } = string.Empty;

    private Customer() { }

    /// <summary>
    /// Factory method. The only way to create a valid Customer.
    /// </summary>
    public static Customer Create(
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
            throw new DomainException("Customer code cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Customer name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(contactEmail))
        {
            throw new DomainException("Customer contact email cannot be empty.");
        }

        var customer = new Customer
        {
            Code         = code.Trim().ToUpperInvariant(),
            Name         = name.Trim(),
            ContactEmail = contactEmail.Trim().ToLowerInvariant(),
            ContactPhone = contactPhone.Trim(),
            Address      = address.Trim()
        };

        customer.Status = CustomerStatus.Active.ToString();
        customer.SetCreated(tenantId, createdBy);

        return customer;
    }

    /// <summary>
    /// Updates customer contact details.
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
            throw new DomainException("Customer name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(contactEmail))
        {
            throw new DomainException("Customer contact email cannot be empty.");
        }

        Name         = name.Trim();
        ContactEmail = contactEmail.Trim().ToLowerInvariant();
        ContactPhone = contactPhone.Trim();
        Address      = address.Trim();
        SetModified(modifiedBy);
    }

    /// <summary>
    /// Deactivates the customer. Inactive customers cannot place orders.
    /// </summary>
    public void Deactivate(Guid modifiedBy)
    {
        if (Status == CustomerStatus.Inactive.ToString())
        {
            throw new DomainException("Customer is already inactive.");
        }

        Status = CustomerStatus.Inactive.ToString();
        SetModified(modifiedBy);
    }

    /// <summary>
    /// Activates the customer.
    /// </summary>
    public void Activate(Guid modifiedBy)
    {
        if (Status == CustomerStatus.Active.ToString())
        {
            throw new DomainException("Customer is already active.");
        }

        Status = CustomerStatus.Active.ToString();
        SetModified(modifiedBy);
    }
}