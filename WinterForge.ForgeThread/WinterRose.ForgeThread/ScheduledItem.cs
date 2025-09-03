using static WinterRose.ForgeThread.ThreadLoom;

namespace WinterRose.ForgeThread
{
    internal class ScheduledItem
    {
        private readonly TaskCompletionSource<object?> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public Action Action { get; }
        public JobPriority Priority { get; }
        public Task Task => completion.Task;

        public ScheduledItem(Action action, JobPriority priority)
        {
            Action = action;
            Priority = priority;
        }

        public void TrySetResult()
        { // If the continuation already set exception, TrySetResult will fail silently
            try { completion.TrySetResult(null); } catch { }
        }

        public void TrySetException(Exception ex)
        {
            try { completion.TrySetException(ex); } catch { }
        }
    }
}
