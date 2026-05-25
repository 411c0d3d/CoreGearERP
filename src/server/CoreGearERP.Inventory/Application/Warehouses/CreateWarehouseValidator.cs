using FluentValidation;

namespace CoreGearERP.Inventory.Application.Warehouses;

/// <summary>
/// Validates CreateWarehouseCommand before it reaches the handler.
/// </summary>
public class CreateWarehouseValidator : AbstractValidator<CreateWarehouseCommand>
{
    /// <summary>
    /// Defines validation rules for CreateWarehouseCommand properties.
    /// </summary>
    public CreateWarehouseValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Warehouse code is required.")
            .MaximumLength(50).WithMessage("Warehouse code cannot exceed 50 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Warehouse name is required.")
            .MaximumLength(200).WithMessage("Warehouse name cannot exceed 200 characters.");

        RuleFor(x => x.Location)
            .MaximumLength(500).WithMessage("Location cannot exceed 500 characters.");
    }
}