using FluentValidation;

namespace CoreGearERP.Sales.Application.Shipments;

/// <summary>
/// Validates ShipOrderCommand before it reaches the handler.
/// </summary>
public class ShipOrderValidator : AbstractValidator<ShipOrderCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShipOrderValidator"/> class.
    /// </summary>
    public ShipOrderValidator()
    {
        RuleFor(x => x.SalesOrderId)
            .NotEmpty().WithMessage("Sales order is required.");

        RuleFor(x => x.WarehouseId)
            .NotEmpty().WithMessage("Warehouse is required.");

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("A shipment must have at least one line.");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.SalesOrderLineId)
                .NotEmpty().WithMessage("Sales order line is required.");

            line.RuleFor(l => l.ProductId)
                .NotEmpty().WithMessage("Product is required.");

            line.RuleFor(l => l.Quantity)
                .GreaterThan(0).WithMessage("Shipment quantity must be greater than zero.");

            line.RuleFor(l => l.UnitCode)
                .NotEmpty().WithMessage("Unit code is required.");
        });
    }
}