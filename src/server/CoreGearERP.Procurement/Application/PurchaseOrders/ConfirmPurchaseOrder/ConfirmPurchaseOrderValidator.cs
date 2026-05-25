using FluentValidation;

namespace CoreGearERP.Procurement.Application.PurchaseOrders.ConfirmPurchaseOrder;

/// <summary>
/// Validates ConfirmPurchaseOrderCommand before it reaches the handler.
/// </summary>
public class ConfirmPurchaseOrderValidator : AbstractValidator<ConfirmPurchaseOrderCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfirmPurchaseOrderValidator"/> class.
    /// </summary>
    public ConfirmPurchaseOrderValidator()
    {
        RuleFor(x => x.PurchaseOrderId)
            .NotEmpty().WithMessage("Purchase order is required.");
    }
}