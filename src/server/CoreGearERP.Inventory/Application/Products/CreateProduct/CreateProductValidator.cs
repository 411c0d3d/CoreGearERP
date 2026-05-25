using FluentValidation;

namespace CoreGearERP.Inventory.Application.Products.CreateProduct;

/// <summary>
/// Validates CreateProductCommand before it reaches the handler.
/// </summary>
public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    /// <summary>
    /// Defines validation rules for creating a product.
    /// </summary>
    public CreateProductValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Product code is required.")
            .MaximumLength(50).WithMessage("Product code cannot exceed 50 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters.");

        RuleFor(x => x.UnitCode)
            .NotEmpty().WithMessage("Unit code is required.")
            .MaximumLength(10).WithMessage("Unit code cannot exceed 10 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");
    }
}