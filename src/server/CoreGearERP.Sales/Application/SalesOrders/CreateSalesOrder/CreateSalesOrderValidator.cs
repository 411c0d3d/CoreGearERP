using FluentValidation;

namespace CoreGearERP.Sales.Application.SalesOrders.CreateSalesOrder;

/// <summary>
/// Validates CreateSalesOrderCommand before it reaches the handler.
/// </summary>
public class CreateSalesOrderValidator : AbstractValidator<CreateSalesOrderCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateSalesOrderValidator"/> class.
    /// </summary>
    public CreateSalesOrderValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer is required.");

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("A sales order must have at least one line item.");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductId)
                .NotEmpty().WithMessage("Product is required on each line.");

            line.RuleFor(l => l.Quantity)
                .GreaterThan(0).WithMessage("Line quantity must be greater than zero.");

            line.RuleFor(l => l.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative.");

            line.RuleFor(l => l.CurrencyCode)
                .NotEmpty().WithMessage("Currency code is required.")
                .Length(3).WithMessage("Currency code must be 3 characters.");

            line.RuleFor(l => l.UnitCode)
                .NotEmpty().WithMessage("Unit code is required.");
        });
    }
}