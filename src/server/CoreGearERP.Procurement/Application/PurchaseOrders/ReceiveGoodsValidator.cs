using FluentValidation;

namespace CoreGearERP.Procurement.Application.PurchaseOrders;

/// <summary>
/// Validates ReceiveGoodsCommand before it reaches the handler.
/// </summary>
public class ReceiveGoodsValidator : AbstractValidator<ReceiveGoodsCommand>
{
    public ReceiveGoodsValidator()
    {
        RuleFor(x => x.PurchaseOrderId)
            .NotEmpty().WithMessage("Purchase order is required.");

        RuleFor(x => x.PurchaseOrderLineId)
            .NotEmpty().WithMessage("Purchase order line is required.");

        RuleFor(x => x.WarehouseId)
            .NotEmpty().WithMessage("Warehouse is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Receipt quantity must be greater than zero.");
    }
}