using FluentValidation;

namespace CoreGearERP.Production.Application.ProductionOrders.CompleteProductionOrder;

/// <summary>
/// Validates CompleteProductionOrderCommand before it reaches the handler.
/// </summary>
public class CompleteProductionOrderValidator : AbstractValidator<CompleteProductionOrderCommand>
{
    /// <summary>
    /// Initializes a new instance of the CompleteProductionOrderValidator class and defines validation rules for the command properties.
    /// </summary>
    public CompleteProductionOrderValidator()
    {
        RuleFor(x => x.ProductionOrderId)
            .NotEmpty().WithMessage("Production order is required.");

        RuleFor(x => x.FinishedGoodsWarehouseId)
            .NotEmpty().WithMessage("Finished goods warehouse is required.");

        RuleFor(x => x.ActualQuantity)
            .GreaterThan(0).WithMessage("Actual quantity must be greater than zero.");

        RuleFor(x => x.ComponentWarehouses)
            .NotEmpty().WithMessage("At least one component warehouse assignment is required.");

        RuleForEach(x => x.ComponentWarehouses).ChildRules(assignment =>
        {
            assignment.RuleFor(a => a.ComponentProductId)
                .NotEmpty().WithMessage("Component product is required.");

            assignment.RuleFor(a => a.WarehouseId)
                .NotEmpty().WithMessage("Warehouse is required for each component.");
        });
    }
}