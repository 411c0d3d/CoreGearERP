using CoreGearERP.Common.Domain.Entities;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Common.Domain.ValueObjects;

namespace CoreGearERP.Production.Domain.Entities;

/// <summary>
/// Represents a single component line in a BillOfMaterials.
/// References a Product by Id only -- no navigation property across module boundary.
/// </summary>
public class BillOfMaterialsLine : BaseEntity
{
    public Guid BillOfMaterialsId { get; private set; }

    /// <summary>
    /// Component product reference by Id. Cross-module -- no navigation property.
    /// </summary>
    public Guid ComponentProductId { get; private set; }

    public string ComponentProductCode { get; private set; } = string.Empty;

    public string ComponentProductName { get; private set; } = string.Empty;

    /// <summary>
    /// Quantity of this component required per production run.
    /// </summary>
    public Quantity QuantityRequired { get; private set; } = null!;

    private BillOfMaterialsLine() { }

    /// <summary>
    /// Factory method. Creates a BOM line with required component quantity.
    /// </summary>
    public static BillOfMaterialsLine Create(
        Guid billOfMaterialsId,
        Guid componentProductId,
        string componentProductCode,
        string componentProductName,
        Quantity quantityRequired,
        Guid tenantId,
        Guid createdBy)
    {
        if (quantityRequired.Value <= 0)
        {
            throw new DomainException("Bill of materials component quantity must be greater than zero.");
        }

        var line = new BillOfMaterialsLine
        {
            BillOfMaterialsId    = billOfMaterialsId,
            ComponentProductId   = componentProductId,
            ComponentProductCode = componentProductCode.Trim().ToUpperInvariant(),
            ComponentProductName = componentProductName.Trim(),
            QuantityRequired     = quantityRequired
        };

        line.Status = "Active";
        line.SetCreated(tenantId, createdBy);

        return line;
    }
}