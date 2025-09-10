namespace WinterRose.ForgeThread
{
    /// <summary>
    /// Simple thread pool that dispatches actions to a set of worker looms.
    /// </summary>
    public sealed class PooledThreadLoom : IDisposable
    {
        private readonly ThreadLoom loom;
        private readonly List<string> workerNames = new();
        private readonly string groupName;

        /// <summary>
        /// Creates a new thread pool with the requested number of workers.
        /// </summary>
        /// <param name="loom">ThreadLoom instance to register workers with.</param>
        /// <param name="poolNamePrefix">Prefix for worker names.</param>
        /// <param name="count">Worker count.</param>
        public PooledThreadLoom(string poolNamePrefix, int count)
        {
            groupName = poolNamePrefix + "-group";
            loom = new ThreadLoom();
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));

            for (int i = 0; i < count; i++)
            {
                var name = $"{poolNamePrefix}-{i}";
                loom.RegisterWorkerThread(name);
                workerNames.Add(name);
            }

            loom.CreateQueueGroup(groupName);

            foreach (var worker in workerNames)
                loom.JoinSharedQueue(worker, groupName);
        }

        /// <summary>
        /// Schedule work on the pool. The work will be executed on one of the underlying workers.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <returns>Task representing completion.</returns>
        public Task Schedule(Action action)
        {
            if (workerNames.Count == 0)
                throw new InvalidOperationException("No worker threads for this pool");
            return loom.InvokeOn(workerNames.First(), action);
        }

        /// <summary>
        /// Disposes the pool and shuts down worker threads.
        /// </summary>
        public void Dispose()
        {
            foreach (var name in workerNames) loom.ShutdownThread(name, TimeSpan.FromSeconds(1));
        }
    }
}
