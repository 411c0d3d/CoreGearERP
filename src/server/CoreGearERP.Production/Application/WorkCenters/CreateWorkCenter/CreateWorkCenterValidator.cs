using FluentValidation;

namespace CoreGearERP.Production.Application.WorkCenters.CreateWorkCenter;

/// <summary>
/// Validates CreateWorkCenterCommand before it reaches the handler.
/// </summary>
public class CreateWorkCenterValidator : AbstractValidator<CreateWorkCenterCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateWorkCenterValidator"/> class.
    /// </summary>
    public CreateWorkCenterValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Work center code is required.")
            .MaximumLength(50).WithMessage("Work center code cannot exceed 50 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Work center name is required.")
            .MaximumLength(200).WithMessage("Work center name cannot exceed 200 characters.");

        RuleFor(x => x.CapacityPerHour)
            .GreaterThan(0).WithMessage("Capacity per hour must be greater than zero.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");
    }
}