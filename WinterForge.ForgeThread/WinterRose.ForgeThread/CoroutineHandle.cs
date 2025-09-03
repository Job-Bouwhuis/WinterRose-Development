namespace WinterRose.ForgeThread
{
    /// <summary>
    /// Lightweight coroutine handle returned by <see cref="ThreadLoom.StartCoroutine(string,System.Collections.IEnumerator)"/>.
    /// </summary>
    public sealed class CoroutineHandle : IDisposable
    {
        private Timer? resumeTimer;
        internal string Name { get; }
        internal IEnumerator<object?> Routine { get; }
        internal bool IsStopped { get; private set; }

        internal CoroutineHandle(string name, IEnumerator<object?> routine, ThreadLoom owner)
        {
            Name = name;
            Routine = routine;
            IsStopped = false;
        }

        internal void AttachTimer(Timer timer)
        {
            resumeTimer = timer;
        }

        internal void MarkCompleted()
        {
            Dispose();
        }

        internal void MarkFaulted(Exception ex)
        {
            Dispose();
        }

        /// <summary>
        /// Stops the coroutine and releases any resources.
        /// </summary>
        public void Dispose()
        {
            IsStopped = true;
            resumeTimer?.Dispose();
            resumeTimer = null;
        }

        /// <summary>
        /// Blocks the calling thread until this coroutine has completed. 
        /// This can be problematic when the coroutine is executed on the a thread,
        /// and this Wait method is also called on the same thread
        /// </summary>
        public void Wait()
        {
            while (!IsStopped) ;
        }
    }
}
