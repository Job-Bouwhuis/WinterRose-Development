using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace WinterRose;

/// <summary>
/// Represents a response to an asynchronous operation across threads. Can be awaited. Awaiting will block until the Result or Exception is set.
/// </summary>
/// <typeparam name="T"></typeparam>
public class Response<T> : INotifyCompletion, IClearDisposable
{
    private readonly object _lock = new object();
    private Action? _continuation;
    private bool _isCompleted;
    private T? _result;
    private Exception? _exception;

    ~Response()
    {
        Dispose();
    }


    /// <summary>
    /// The result of the response.
    /// </summary>
    public T Result
    {
        get
        {
            if (!_isCompleted)
            {
                throw new InvalidOperationException("Response has not completed yet.");
            }
            if (_exception != null)
            {
                throw _exception;
            }
            return _result!;
        }
    }

    /// <summary>
    /// Whether the response has been completed.
    /// </summary>
    public bool IsCompleted => _isCompleted;

    /// <summary>
    /// Whether the response has faulted.
    /// </summary>
    public bool IsFaulted => _exception != null;

    /// <summary>
    /// The exception that caused the response to fault. If null, the response was successful or has not yet completed.
    /// </summary>
    public Exception? Exception => _exception;

    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Gets the awaiter.
    /// </summary>
    /// <returns></returns>
    public Response<T> GetAwaiter() => this;

    public bool Wait(TimeSpan? timeout = null)
    {
        Task t = Task.Run(() =>
        {
            while (!_isCompleted)
                continue;
        });

        return t.Wait(timeout is null ? Timeout.Infinite : (int)timeout.Value.TotalMilliseconds);
    }

    /// <summary>
    /// Sets the result of the response.
    /// </summary>
    /// <param name="result"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void SetResult(T result)
    {
        lock (_lock)
        {
            if (_isCompleted)
                throw new InvalidOperationException("Result has already been set.");
            _result = result;
            _isCompleted = true;
            _continuation?.Invoke();
        }
    }

    /// <summary>
    /// Gets the result of the response.
    /// </summary>
    /// <returns></returns>
    public T GetResult() => Result;

    /// <summary>
    /// Sets the exception of the response.
    /// </summary>
    /// <param name="ex"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void SetException(Exception ex)
    {
        lock (_lock)
        {
            if (_isCompleted)
                throw new InvalidOperationException("Result has already been set.");
            _exception = ex;
            _isCompleted = true;
            _continuation?.Invoke();
        }
    }

    /// <summary>
    /// Called when the response is completed.
    /// </summary>
    /// <param name="continuation"></param>
    public void OnCompleted(Action continuation)
    {
        lock (_lock)
        {
            if (_isCompleted)
            {
                continuation();
            }
            else
            {
                _continuation = continuation;
            }
        }
    }

    public void Dispose()
    {
        // remove _continuation to prevent memory leaks
        _continuation = null;

        // remove _result from memory
        _result = default!;

        GC.SuppressFinalize(this);
    }
}
