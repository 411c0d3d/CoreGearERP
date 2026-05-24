using CoreGearERP.Common.Application.Interfaces;

namespace CoreGearERP.Host.Extensions;

/// <summary>
/// Maps Result to IResult for minimal API endpoints.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Maps a Result to an appropriate IResult for minimal API endpoints. On success, returns 200 OK with the value. On failure,
    /// </summary>
    /// <param name="result">The Result to map.</param>
    /// <param name="onSuccess">An optional function to map the success value to a custom IResult. If not provided, defaults to 200 OK with the value.</param>
    /// <returns>returns 404 Not Found for not found errors, 400 Bad Request for domain errors, and 500 Internal Server Error for other errors.</returns>
    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult>? onSuccess = null)
    {
        if (result.IsSuccess)
        {
            return onSuccess is not null
                ? onSuccess(result.Value!)
                : Results.Ok(result.Value);
        }

        return result.ErrorType switch
        {
            ResultErrorType.NotFound => Results.NotFound(new { error = result.Error }),
            ResultErrorType.DomainError => Results.BadRequest(new { error = result.Error }),
            _ => Results.StatusCode(500)
        };
    }
}