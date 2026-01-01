using System.Collections;

namespace WinterRose.ForgeThread;

/// <summary>
/// Lightweight coroutine handle returned by <see cref="ThreadLoom.StartCoroutine(string,System.Collections.IEnumerator)"/>.
/// </summary>
public sealed class CoroutineHandle : IDisposable
{
    private Timer? resumeTimer;
    private readonly TaskCompletionSource<object?> completionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

    internal string Name { get; }
    internal IEnumerator Routine { get; }
    public bool IsComplete { get; private set; }

    /// <summary>
    /// The Task completes when the coroutine finishes (result = last yielded value or null).
    /// </summary>
    public Task<object?> Task => completionSource.Task;

    /// <summary>
    /// The last value yielded by the coroutine (updated each tick if the coroutine yields a non-TimeSpan value).
    /// </summary>
    public object? LastYield { get; private set; }

    internal CoroutineHandle(string name, IEnumerator routine, ThreadLoom owner)
    {
        Name = name;
        Routine = routine;
        IsComplete = false;
        LastYield = null;
    }

    internal void AttachTimer(Timer timer)
    {
        resumeTimer = timer;
    }

    internal void MarkCompleted()
    {
        if (!IsComplete)
        {
            completionSource.TrySetResult(LastYield);
            Dispose();
        }
    }

    internal void MarkFaulted(Exception ex)
    {
        if (!IsComplete)
        {
            completionSource.TrySetException(ex);
            Dispose();
        }
    }

    /// <summary>
    /// Stops the coroutine and releases any resources.
    /// </summary>
    public void Dispose()
    {
        if (IsComplete) return;
        IsComplete = true;
        resumeTimer?.Dispose();
        resumeTimer = null;
    }

    /// <summary>
    /// Blocks the calling thread until this coroutine has completed.
    /// </summary>
    public void Wait()
    {
        Task.Wait();
    }

    // internal helper for ThreadLoom to update last yield
    internal void UpdateLastYield(object? value)
    {
        LastYield = value;
    }

    public void ContinueWith(Action<Task> value)
    {
        Task.ContinueWith(value);
    }
}
