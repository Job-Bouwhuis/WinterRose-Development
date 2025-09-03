using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

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

        public LoomThread(string name, bool isHostedMain)
        {
            Name = name;
            IsHostedMain = isHostedMain;
        }

        public void AttachThread(Thread thread)
        {
            ThreadInstance = thread;
        }

        public void BeginNextTick()
        {
            //Console.WriteLine("Beginning next tick on thread: " + Name);
            MoveNextTickItemsToMainQueue();
        }

        private bool TryEnqueue(ScheduledItem item)
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

        public void EnqueueNextTick(ScheduledItem item)
        {
            nextTickQueue.Enqueue(item);
        }

        public void MoveNextTickItemsToMainQueue()
        {
            while (nextTickQueue.TryDequeue(out var item))
            {
                TryEnqueue(item);
            }
        }

        public bool TryDequeue(out ScheduledItem? item)
        {
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
