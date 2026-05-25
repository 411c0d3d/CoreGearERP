// CoreGearERP.Common/Application/Interfaces/Result.cs

namespace CoreGearERP.Common.Application.Interfaces;

/// <summary>
/// Wraps a command or query result with success or failure state.
/// Prevents exceptions from propagating through the middleware stack.
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public ResultErrorType ErrorType { get; }

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <param name="value">The value of the successful result.</param>
    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
    }

    /// <summary>
    /// Creates a failure result with the specified error message and type.
    /// </summary>
    /// <param name="error">The error message describing the failure.</param>
    /// <param name="errorType">The type of error (e.g., domain error, not found, unexpected).</param>
    private Result(string error, ResultErrorType errorType)
    {
        IsSuccess = false;
        Error = error;
        ErrorType = errorType;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string error) => new(error, ResultErrorType.DomainError);
    public static Result<T> NotFound(string error) => new(error, ResultErrorType.NotFound);
    public static Result<T> Unexpected(string error) => new(error, ResultErrorType.Unexpected);
}