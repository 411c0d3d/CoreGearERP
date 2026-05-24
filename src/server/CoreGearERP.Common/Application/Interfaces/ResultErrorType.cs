namespace CoreGearERP.Common.Application.Interfaces;

/// <summary>
/// Defines the type of error that occurred during the execution of a command or query.
/// </summary>
public enum ResultErrorType
{
    DomainError,
    NotFound,
    Unexpected
}