using FluentValidation;

namespace CoreGearERP.Inventory.Application.StockItems;

/// <summary>
/// Validates CreateStockItemCommand before it reaches the handler.
/// </summary>
public class CreateStockItemValidator : AbstractValidator<CreateStockItemCommand>
{
    public CreateStockItemValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required.");

        RuleFor(x => x.WarehouseId)
            .NotEmpty().WithMessage("WarehouseId is required.");

        RuleFor(x => x.UnitCode)
            .NotEmpty().WithMessage("Unit code is required.")
            .MaximumLength(10).WithMessage("Unit code cannot exceed 10 characters.");
    }
}