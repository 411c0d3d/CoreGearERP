namespace CoreGearERP.Common.Domain.Exceptions;

/// <summary>
/// Thrown when a domain rule is violated.
/// Caught by the exception middleware and returned as 400 Bad Request.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}