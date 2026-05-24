using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Common.Domain.Exceptions;
using FluentValidation;

namespace CoreGearERP.Host.Infrastructure.Behaviors;

/// <summary>
/// Runs FluentValidation validators before every command and query.
/// If validation fails throws DomainException which maps to 400 Bad Request.
/// </summary>
public class ValidationBehavior<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationBehavior{TRequest, TResult}"/> class.
    /// </summary>
    /// <param name="validators">The validators.</param>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    /// <summary>
    /// Handles the validation.
    /// </summary>
    public async Task<TResult> Handle(
        TRequest request,
        Func<Task<TResult>> next,
        CancellationToken cancellationToken = default)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context  = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
        {
            var errors = string.Join(" | ", failures.Select(f => f.ErrorMessage));
            throw new DomainException(errors);
        }

        return await next();
    }
}