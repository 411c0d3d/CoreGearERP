using CoreGearERP.Common.Application.Interfaces;

namespace CoreGearERP.Inventory.Application.Commands;

/// <summary>
/// Command to create a new product in the inventory.
/// </summary>
public record CreateProductCommand(
    string Code,
    string Name,
    string UnitCode,
    string Description = "") : ICommand<Guid>;