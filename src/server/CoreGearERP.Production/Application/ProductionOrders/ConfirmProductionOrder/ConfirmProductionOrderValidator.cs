using FluentValidation;

namespace CoreGearERP.Production.Application.ProductionOrders.ConfirmProductionOrder;

/// <summary>
/// Validates ConfirmProductionOrderCommand before it reaches the handler.
/// </summary>
public class ConfirmProductionOrderValidator : AbstractValidator<ConfirmProductionOrderCommand>
{
    /// <summary>
    /// Initializes a new instance of the ConfirmProductionOrderValidator class.
    /// </summary>
    public ConfirmProductionOrderValidator()
    {
        RuleFor(x => x.ProductionOrderId)
            .NotEmpty().WithMessage("Production order is required.");

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