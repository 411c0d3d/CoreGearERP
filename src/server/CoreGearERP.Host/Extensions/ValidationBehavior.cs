using FluentValidation;
using CoreGearERP.Common.Domain.Exceptions;

namespace CoreGearERP.Host.Extensions;

/// <summary>
/// Pipeline behavior that validates every command before it reaches a handler.
/// Throws DomainException on failure which the exception middleware maps to 400.
/// </summary>
public class ValidationBehavior<TRequest, TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, Func<Task<TResponse>> next)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

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