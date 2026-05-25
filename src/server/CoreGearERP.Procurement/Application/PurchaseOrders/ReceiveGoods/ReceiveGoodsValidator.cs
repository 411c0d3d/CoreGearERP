using FluentValidation;

namespace CoreGearERP.Procurement.Application.PurchaseOrders.ReceiveGoods;

/// <summary>
/// Validates ReceiveGoodsCommand before it reaches the handler.
/// </summary>
public class ReceiveGoodsValidator : AbstractValidator<ReceiveGoodsCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReceiveGoodsValidator"/> class.
    /// </summary>
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