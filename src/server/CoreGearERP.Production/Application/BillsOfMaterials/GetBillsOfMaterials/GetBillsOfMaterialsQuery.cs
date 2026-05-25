using CoreGearERP.Common.Application.Interfaces;

namespace CoreGearERP.Production.Application.BillsOfMaterials.GetBillsOfMaterials;

/// <summary>
/// Query to retrieve all BillsOfMaterials for the current tenant.
/// </summary>
public record GetBillsOfMaterialsQuery : IQuery<IReadOnlyList<BillOfMaterialsDto>>;

/// <summary>
/// Read model for BillOfMaterials display.
/// </summary>
public record BillOfMaterialsDto(
    Guid Id,
    Guid FinishedProductId,
    string FinishedProductCode,
    string FinishedProductName,
    string Version,
    string Status,
    int ComponentCount);