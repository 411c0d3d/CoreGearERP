using CoreGearERP.Common.Domain.Entities;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Inventory.Domain.Enums;

namespace CoreGearERP.Inventory.Domain.Entities;

/// <summary>Represents a physical warehouse location that holds stock.</summary>
public class Warehouse : BaseEntity
{
    /// <summary>Human-facing warehouse code. Unique per tenant.</summary>
    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Location { get; private set; } = string.Empty;

    private Warehouse() { }

    /// <summary>Factory method. The only way to create a valid Warehouse.</summary>
    public static Warehouse Create(
        string code,
        string name,
        string location,
        Guid tenantId,
        Guid createdBy)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException("Warehouse code cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Warehouse name cannot be empty.");
        }

        var warehouse = new Warehouse
        {
            Code     = code.Trim().ToUpperInvariant(),
            Name     = name.Trim(),
            Location = location.Trim()
        };

        warehouse.Status = WarehouseStatus.Active.ToString();
        warehouse.SetCreated(tenantId, createdBy);

        return warehouse;
    }

    /// <summary>Deactivates the warehouse. Inactive warehouses cannot receive stock.</summary>
    public void Deactivate(Guid modifiedBy)
    {
        if (Status == WarehouseStatus.Inactive.ToString())
        {
            throw new DomainException("Warehouse is already inactive.");
        }

        Status = WarehouseStatus.Inactive.ToString();
        SetModified(modifiedBy);
    }

    /// <summary>Activates the warehouse.</summary>
    public void Activate(Guid modifiedBy)
    {
        if (Status == WarehouseStatus.Active.ToString())
        {
            throw new DomainException("Warehouse is already active.");
        }

        Status = WarehouseStatus.Active.ToString();
        SetModified(modifiedBy);
    }
}