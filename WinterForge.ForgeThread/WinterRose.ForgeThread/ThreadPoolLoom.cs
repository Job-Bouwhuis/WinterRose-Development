namespace WinterRose.ForgeThread
{
    /// <summary>
    /// Simple thread pool that dispatches actions to a set of worker looms.
    /// </summary>
    public sealed class ThreadPoolLoom : IDisposable
    {
        private readonly ThreadLoom loomOwner;
        private readonly List<string> workerNames = new();
        private int roundRobinIndex;

        /// <summary>
        /// Creates a new thread pool with the requested number of workers.
        /// </summary>
        /// <param name="loomOwner">ThreadLoom instance to register workers with.</param>
        /// <param name="poolNamePrefix">Prefix for worker names.</param>
        /// <param name="count">Worker count.</param>
        public ThreadPoolLoom(ThreadLoom loomOwner, string poolNamePrefix, int count)
        {
            this.loomOwner = loomOwner ?? throw new ArgumentNullException(nameof(loomOwner));
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));

            for (int i = 0; i < count; i++)
            {
                var name = $"{poolNamePrefix}-{i}";
                loomOwner.RegisterWorkerThread(name);
                workerNames.Add(name);
            }
        }

        /// <summary>
        /// Schedule work on the pool. The work will be executed on one of the underlying workers.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <returns>Task representing completion.</returns>
        public Task Schedule(Action action)
        {
            var name = PickWorker();
            return loomOwner.InvokeOn(name, action);
        }

        private string PickWorker()
        {
            int idx = Interlocked.Increment(ref roundRobinIndex);
            return workerNames[Math.Abs(idx) % workerNames.Count];
        }

        /// <summary>
        /// Disposes the pool and shuts down worker threads.
        /// </summary>
        public void Dispose()
        {
            foreach (var name in workerNames) loomOwner.ShutdownThread(name, TimeSpan.FromSeconds(1));
        }
    }
}
