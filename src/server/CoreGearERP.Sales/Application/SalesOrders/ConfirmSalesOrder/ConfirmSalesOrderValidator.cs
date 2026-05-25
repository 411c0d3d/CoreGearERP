using FluentValidation;

namespace CoreGearERP.Sales.Application.SalesOrders.ConfirmSalesOrder;

/// <summary>
/// Validates ConfirmSalesOrderCommand before it reaches the handler.
/// </summary>
public class ConfirmSalesOrderValidator : AbstractValidator<ConfirmSalesOrderCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfirmSalesOrderValidator"/> class.
    /// </summary>
    public ConfirmSalesOrderValidator()
    {
        RuleFor(x => x.SalesOrderId)
            .NotEmpty().WithMessage("Sales order is required.");

        RuleFor(x => x.WarehouseId)
            .NotEmpty().WithMessage("Warehouse is required.");
    }
}