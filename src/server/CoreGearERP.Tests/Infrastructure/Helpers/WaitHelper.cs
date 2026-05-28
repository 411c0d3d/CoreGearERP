namespace CoreGearERP.Tests.Infrastructure.Helpers;

/// <summary>
/// Provides polling utilities for asserting async side effects produced by message consumers.
/// </summary>
public static class WaitHelper
{
    /// <summary>
    /// Polls <paramref name="condition"/> until it returns true or <paramref name="timeout"/> elapses.
    /// Throws <see cref="TimeoutException"/> when the deadline is exceeded.
    /// </summary>
    /// <param name="condition">Async predicate to evaluate on each poll.</param>
    /// <param name="timeout">Maximum time to wait. Defaults to 10 seconds.</param>
    /// <param name="interval">Delay between polls. Defaults to 300 milliseconds.</param>
    /// <param name="failureMessage">Message included in the <see cref="TimeoutException"/> on failure.</param>
    public static async Task WaitForConditionAsync(
        Func<Task<bool>> condition,
        TimeSpan? timeout = null,
        TimeSpan? interval = null,
        string? failureMessage = null)
    {
        var deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(20));
        var pollInterval = interval ?? TimeSpan.FromMilliseconds(300);

        while (DateTime.UtcNow < deadline)
        {
            if (await condition())
            {
                return;
            }

            await Task.Delay(pollInterval);
        }

        throw new TimeoutException(
            failureMessage ?? "Condition was not met within the allowed timeout.");
    }
}