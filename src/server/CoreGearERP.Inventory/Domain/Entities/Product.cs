using CoreGearERP.Common.Domain.Entities;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Inventory.Domain.Enums;

namespace CoreGearERP.Inventory.Domain.Entities;

/// <summary>
/// Represents a product that can be stocked, procured, and used in production.
/// Root entity of the Inventory module.
/// </summary>
public class Product : BaseEntity
{
    /// <summary>
    /// Human-facing product code. Unique per tenant. Never the PK.
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Unit of measure for this product. KG, PCS, LTR etc.
    /// </summary>
    public string UnitCode { get; private set; } = string.Empty;

    // Private constructor for EF Core.
    private Product()
    {
    }

    /// <summary>
    /// Factory method. The only way to create a valid Product.
    /// </summary>
    public static Product Create(
        string code,
        string name,
        string unitCode,
        Guid tenantId,
        Guid createdBy,
        string description = "")
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException("Product code cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Product name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(unitCode))
        {
            throw new DomainException("Unit code cannot be empty.");
        }

        var product = new Product
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = description.Trim(),
            UnitCode = unitCode.Trim().ToUpperInvariant(),
        };

        product.Status = ProductStatus.Active.ToString();
        product.SetCreated(tenantId, createdBy);

        return product;
    }

    /// <summary>
    /// Updates product details. Only name and description are mutable after creation.
    /// </summary>
    public void Update(string name, string description, Guid modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Product name cannot be empty.");
        }

        Name = name.Trim();
        Description = description.Trim();
        SetModified(modifiedBy);
    }

    /// <summary>
    /// Marks the product as inactive. Inactive products cannot receive stock.
    /// </summary>
    public void Deactivate(Guid modifiedBy)
    {
        if (Status == ProductStatus.Discontinued.ToString())
        {
            throw new DomainException("A discontinued product cannot be deactivated.");
        }

        Status = ProductStatus.Inactive.ToString();
        SetModified(modifiedBy);
    }

    /// <summary>
    /// Marks the product as discontinued. This is irreversible.
    /// </summary>
    public void Discontinue(Guid modifiedBy)
    {
        Status = ProductStatus.Discontinued.ToString();
        SetModified(modifiedBy);
    }

    /// <summary>
    /// Soft deletes the product.
    /// </summary>
    public void Delete(Guid deletedBy)
    {
        if (Status == ProductStatus.Active.ToString())
        {
            throw new DomainException("An active product cannot be deleted. Deactivate it first.");
        }

        SetDeleted(deletedBy);
    }
}