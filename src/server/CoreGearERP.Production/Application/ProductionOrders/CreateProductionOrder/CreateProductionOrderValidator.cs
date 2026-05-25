using FluentValidation;

namespace CoreGearERP.Production.Application.ProductionOrders.CreateProductionOrder;

/// <summary>
/// Validates CreateProductionOrderCommand before it reaches the handler.
/// </summary>
public class CreateProductionOrderValidator : AbstractValidator<CreateProductionOrderCommand>
{
    /// <summary>
    /// Initializes a new instance of the CreateProductionOrderValidator class.
    /// </summary>
    public CreateProductionOrderValidator()
    {
        RuleFor(x => x.BillOfMaterialsId)
            .NotEmpty().WithMessage("Bill of materials is required.");

        RuleFor(x => x.WorkCenterId)
            .NotEmpty().WithMessage("Work center is required.");

        RuleFor(x => x.PlannedQuantity)
            .GreaterThan(0).WithMessage("Planned quantity must be greater than zero.");

        RuleFor(x => x.UnitCode)
            .NotEmpty().WithMessage("Unit code is required.");
    }
}