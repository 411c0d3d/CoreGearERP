using FluentValidation;

namespace CoreGearERP.Sales.Application.Customers.CreateCustomer;

/// <summary>
/// Validates CreateCustomerCommand before it reaches the handler.
/// </summary>
public class CreateCustomerValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Customer code is required.")
            .MaximumLength(50).WithMessage("Customer code cannot exceed 50 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(200).WithMessage("Customer name cannot exceed 200 characters.");

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