using CoreGearERP.Common.Application.Interfaces;

namespace CoreGearERP.Production.Application.BillsOfMaterials.CreateBillOfMaterials;

/// <summary>
/// Command to create a BillOfMaterials with component lines.
/// </summary>
public record CreateBillOfMaterialsCommand(
    Guid FinishedProductId,
    string FinishedProductCode,
    string FinishedProductName,
    string Version,
    string Notes,
    IReadOnlyList<CreateBillOfMaterialsLineDto> Lines) : ICommand<Guid>;

/// <summary>
/// Component line input for BillOfMaterials creation.
/// </summary>
public record CreateBillOfMaterialsLineDto(
    Guid ComponentProductId,
    string ComponentProductCode,
    string ComponentProductName,
    decimal Quantity,
    string UnitCode);