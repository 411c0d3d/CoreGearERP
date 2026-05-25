using FluentValidation;

namespace CoreGearERP.Production.Application.BillsOfMaterials.CreateBillOfMaterials;

/// <summary>
/// Validates CreateBillOfMaterialsCommand before it reaches the handler.
/// </summary>
public class CreateBillOfMaterialsValidator : AbstractValidator<CreateBillOfMaterialsCommand>
{

    /// <summary>
    /// Initializes a new instance of the CreateBillOfMaterialsValidator class and defines validation rules for the command properties and its component lines.
    /// </summary>
    public CreateBillOfMaterialsValidator()
    {
        RuleFor(x => x.FinishedProductId)
            .NotEmpty().WithMessage("Finished product is required.");

        RuleFor(x => x.FinishedProductCode)
            .NotEmpty().WithMessage("Finished product code is required.");

        RuleFor(x => x.Version)
            .NotEmpty().WithMessage("Bill of materials version is required.")
            .MaximumLength(50).WithMessage("Version cannot exceed 50 characters.");

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("Bill of materials must have at least one component line.");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ComponentProductId)
                .NotEmpty().WithMessage("Component product is required.");

            line.RuleFor(l => l.Quantity)
                .GreaterThan(0).WithMessage("Component quantity must be greater than zero.");

            line.RuleFor(l => l.UnitCode)
                .NotEmpty().WithMessage("Unit code is required.");
        });
    }
}