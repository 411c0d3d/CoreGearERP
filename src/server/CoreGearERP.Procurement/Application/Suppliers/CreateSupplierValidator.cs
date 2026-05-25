using FluentValidation;

namespace CoreGearERP.Procurement.Application.Suppliers;

/// <summary>
/// Validates CreateSupplierCommand before it reaches the handler.
/// </summary>
public class CreateSupplierValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Supplier code is required.")
            .MaximumLength(50).WithMessage("Supplier code cannot exceed 50 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Supplier name is required.")
            .MaximumLength(200).WithMessage("Supplier name cannot exceed 200 characters.");

        RuleFor(x => x.ContactEmail)
            .NotEmpty().WithMessage("Contact email is required.")
            .EmailAddress().WithMessage("Contact email must be a valid email address.")
            .MaximumLength(200).WithMessage("Contact email cannot exceed 200 characters.");

        RuleFor(x => x.ContactPhone)
            .MaximumLength(50).WithMessage("Contact phone cannot exceed 50 characters.");

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Address cannot exceed 500 characters.");
    }
}