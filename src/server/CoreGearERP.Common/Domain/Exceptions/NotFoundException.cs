namespace CoreGearERP.Common.Domain.Exceptions;

/// <summary>
/// Thrown when a requested entity does not exist or does not belong to the current tenant.
/// Deliberately vague -- we do not confirm whether an entity exists in another tenant.
/// Caught by exception middleware and returned as 404 Not Found.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found.")
    {
    }
}