using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using WinterRose.Recordium;

namespace WinterRose.ForgeThread
{
    internal class LoomThread
    {
        private readonly ConcurrentQueue<ScheduledItem> highQueue = new();
        private readonly ConcurrentQueue<ScheduledItem> normalQueue = new();
        private readonly ConcurrentQueue<ScheduledItem> lowQueue = new();

        private readonly ConcurrentQueue<ScheduledItem> nextTickQueue = new();

        //private readonly SemaphoreSlim queueSignal = new(0);
        private readonly CancellationTokenSource cancellation = new();

        public string Name { get; }
        public bool IsHostedMain { get; }
        public Thread? ThreadInstance { get; private set; }
        public Log Log { get; }

        public LoomThread(string name, bool isHostedMain)
        {
            Name = name;
            IsHostedMain = isHostedMain;
            Log = new Log("ThreadLoom.WorkerThread:" + Name);
        }

        public void AttachThread(Thread thread)
        {
            ThreadInstance = thread;
        }

        public void BeginNextTick()
        {
            MoveNextTickItemsToMainQueue();
        }

        private bool TryEnqueueThisTick(ScheduledItem item)
        {
            switch (item.Priority)
            {
                case JobPriority.High: highQueue.Enqueue(item); break;
                case JobPriority.Normal: normalQueue.Enqueue(item); break;
                default: lowQueue.Enqueue(item); break;
            }

            // signal worker if not hosted main
            //if (!IsHostedMain) queueSignal.Release();
            return true;
        }

        public void Enqueue(ScheduledItem item)
        {
            nextTickQueue.Enqueue(item);
        }

        public void MoveNextTickItemsToMainQueue()
        {
            while (nextTickQueue.TryDequeue(out var item))
            {
                TryEnqueueThisTick(item);
            }
        }

        public bool TryDequeue(out ScheduledItem? item)
        {
            item = null;

            if (Paused)
                return false;

            if (highQueue.TryDequeue(out item)) return true;
            if (normalQueue.TryDequeue(out item)) return true;
            if (lowQueue.TryDequeue(out item)) return true;
            item = null;
            return false;
        }

        public bool TryTake([NotNullWhen(true)] out ScheduledItem? item)
        {
            item = null;

            if (Paused)
                return false;

            // try to pop highest priority first
            if (highQueue.TryDequeue(out item)) return true;
            if (normalQueue.TryDequeue(out item)) return true;
            if (lowQueue.TryDequeue(out item)) return true;
            return false;
        }

        public bool IsCancellationRequested => cancellation.IsCancellationRequested;
        public bool IsShutdown => /* queueSignal == null || */cancellation.IsCancellationRequested;

        public bool Paused { get; internal set; }

        public void RequestShutdown()
        {
            cancellation.Cancel();
        }
    }

}
