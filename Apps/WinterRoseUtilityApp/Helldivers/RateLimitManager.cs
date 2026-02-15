using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using WinterRose.Recordium;

namespace WinterRoseUtilityApp.Helldivers;

/// <summary>
/// Manages API rate limiting for Helldivers API client.
/// Rate limit: 5 requests per 10 seconds (1 per 2 seconds)
/// </summary>
internal class RateLimitManager
{
    private readonly Log _log;
    private readonly Queue<Func<Task>> _requestQueue = new();
    //private readonly SemaphoreSlim _queueSemaphore = new(1, 1);
    private DateTime _lastRequestTime = DateTime.UtcNow.AddSeconds(-3); // Allow first request immediately
    private int _remainingRequests = 5;
    private DateTime _windowResetTime = DateTime.UtcNow.AddSeconds(10);
    
    private const int RequestsPerWindow = 2;
    private const int WindowSeconds = 10;
    private const int MinMillisecondsBetweenRequests = 2000; // 2 seconds for safety margin

    public RateLimitManager(Log log)
    {
        _log = log;
    }

    /// <summary>
    /// Executes an API request with rate limiting
    /// </summary>
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> request, string requestName = "")
    {
        const int retryDelaySeconds = 10;

        while (true)
        {
            await WaitForRateLimit();

            _log.Debug($"Executing API request: {requestName}");
            var task = request();
            await task;

            // Check if the task faulted
            if (task.IsFaulted && task.Exception != null)
            {
                if (ContainsRateLimitException(task.Exception))
                {
                    _log.Warning($"Rate limit hit on request {requestName}. Waiting {retryDelaySeconds}s before retrying...");
                    await Task.Delay(TimeSpan.FromSeconds(retryDelaySeconds));
                    continue;
                }

                throw task.Exception.Flatten();
            }

            _lastRequestTime = DateTime.UtcNow;
            _remainingRequests--;

            _log.Debug($"Request successful: {requestName} (Remaining: {_remainingRequests})");
            return task.Result;
        }
    }

    // Helper to check for a 429 inside AggregateException
    private bool ContainsRateLimitException(AggregateException aggregate)
    {
        foreach (var ex in aggregate.Flatten().InnerExceptions)
        {
            if (ex is HttpRequestException httpEx && httpEx.StatusCode == (HttpStatusCode)429)
                return true;

            if (ex.Message.Contains("429") || ex.Message.Contains("Too Many Requests"))
                return true;
        }
        return false;
    }



    // Helper method to determine if an exception is a 429
    private bool IsRateLimitException(Exception ex)
    {
        // Example for HttpRequestException with StatusCode
        if (ex is HttpRequestException httpEx && httpEx.StatusCode == (HttpStatusCode)429)
            return true;

        // Or if your library wraps it differently, check the message
        if (ex.Message.Contains("429") || ex.Message.Contains("Too Many Requests"))
            return true;

        return false;
    }


    /// <summary>
    /// Updates rate limit info from response headers
    /// Should be called after receiving an API response
    /// </summary>
    public void UpdateFromHeaders(int? remaining, int? limit, int? retryAfter)
    {
        if (remaining.HasValue)
        {
            _remainingRequests = remaining.Value;
            _log.Debug($"Rate limit updated: {_remainingRequests}/{limit ?? RequestsPerWindow} remaining");
        }

        if (retryAfter.HasValue)
        {
            var delayUntil = DateTime.UtcNow.AddSeconds(retryAfter.Value);
            if (delayUntil > _windowResetTime)
            {
                _windowResetTime = delayUntil;
                _log.Warning($"Rate limit hit - waiting {retryAfter} seconds before next request");
            }
        }
    }

    /// <summary>
    /// Waits if necessary to respect rate limits
    /// </summary>
    private async Task WaitForRateLimit()
    {
        // Check if we've exceeded the window
        var now = DateTime.UtcNow;
        if (now >= _windowResetTime)
        {
            _remainingRequests = RequestsPerWindow;
            _windowResetTime = now.AddSeconds(WindowSeconds);
        }

        // Check if we have remaining requests
        if (_remainingRequests <= 0)
        {
            var waitTime = _windowResetTime - now;
            _log.Warning($"Rate limit exhausted - waiting {waitTime.TotalSeconds:F1} seconds");
            await Task.Delay(waitTime);
            _remainingRequests = RequestsPerWindow;
            _windowResetTime = DateTime.UtcNow.AddSeconds(WindowSeconds);
            return;
        }

        // Check time since last request
        var timeSinceLastRequest = now - _lastRequestTime;
        if (timeSinceLastRequest.TotalMilliseconds < MinMillisecondsBetweenRequests)
        {
            var delayMs = MinMillisecondsBetweenRequests - (int)timeSinceLastRequest.TotalMilliseconds;
            _log.Debug($"Throttling request - waiting {delayMs}ms");
            await Task.Delay(delayMs);
        }
    }

    /// <summary>
    /// Get current rate limit status
    /// </summary>
    public (int remaining, int total, TimeSpan windowResetIn) GetStatus()
    {
        var resetIn = _windowResetTime - DateTime.UtcNow;
        return (_remainingRequests, RequestsPerWindow, resetIn > TimeSpan.Zero ? resetIn : TimeSpan.Zero);
    }
}
