using CoreGearERP.Common.Domain.Entities;
using CoreGearERP.Common.Domain.Exceptions;

namespace CoreGearERP.Production.Domain.Entities;

/// <summary>
/// Blueprint for producing a finished good.
/// References the finished Product by Id only -- no navigation property across module boundary.
/// </summary>
public class BillOfMaterials : BaseEntity
{
    /// <summary>
    /// The finished good this BOM produces. Cross-module reference by Id only.
    /// </summary>
    public Guid FinishedProductId { get; private set; }

    public string FinishedProductCode { get; private set; } = string.Empty;

    public string FinishedProductName { get; private set; } = string.Empty;

    public string Version { get; private set; } = string.Empty;

    public string Notes { get; private set; } = string.Empty;

    private readonly List<BillOfMaterialsLine> _lines = [];
    public IReadOnlyList<BillOfMaterialsLine> Lines => _lines.AsReadOnly();

    private BillOfMaterials()
    {
    }

    /// <summary>
    /// Factory method. Creates a BillOfMaterials in Active status.
    /// </summary>
    public static BillOfMaterials Create(
        Guid finishedProductId,
        string finishedProductCode,
        string finishedProductName,
        string version,
        string notes,
        Guid tenantId,
        Guid createdBy)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            throw new DomainException("Bill of materials version cannot be empty.");
        }

        var bom = new BillOfMaterials
        {
            FinishedProductId = finishedProductId,
            FinishedProductCode = finishedProductCode.Trim().ToUpperInvariant(),
            FinishedProductName = finishedProductName.Trim(),
            Version = version.Trim(),
            Notes = notes.Trim()
        };

        bom.Status = "Active";
        bom.SetCreated(tenantId, createdBy);

        return bom;
    }

    /// <summary>
    /// Adds a component line to the BillOfMaterials.
    /// </summary>
    public void AddLine(BillOfMaterialsLine line, Guid modifiedBy)
    {
        var duplicate = _lines.Any(l => l.ComponentProductId == line.ComponentProductId);

        if (duplicate)
        {
            throw new DomainException(
                $"Component '{line.ComponentProductCode}' already exists in this bill of materials.");
        }

        _lines.Add(line);
        SetModified(modifiedBy);
    }

    /// <summary>
    /// Validates the BillOfMaterials has at least one component before use in production.
    /// </summary>
    public void ValidateForProduction()
    {
        if (_lines.Count == 0)
        {
            throw new DomainException("Bill of materials must have at least one component before use in production.");
        }
    }
}