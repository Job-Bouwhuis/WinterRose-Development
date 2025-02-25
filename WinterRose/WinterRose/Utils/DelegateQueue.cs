using System;
using System.Collections.Concurrent;

namespace WinterRose
{
    /// <summary>
    /// A queue that invokes delegates in order of when they were added
    /// </summary>
    public class DelegateQueue
    {
        private readonly ConcurrentStack<DelegateParams> queue = [];

        /// <summary>
        /// Whether or not the queue is running
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Adds the given delegate to the queue
        /// </summary>
        /// <param name="item"></param>
        public void Add(Delegate item) => queue.Push(item);
        /// <summary>
        /// Starts the queue
        /// </summary>
        /// <param name="autoStop"></param>
        public void StartQueue(bool autoStop = true)
        {
            IsRunning = true;

            while (IsRunning)
            {
                if (queue.TryPop(out var item))
                {
                    item.Invoke();
                }
                else if (autoStop)
                {
                    IsRunning = false;
                }
            }
        }
        /// <summary>
        /// Stops the queue
        /// </summary>
        public void StopQueue() => IsRunning = false;
        /// <summary>
        /// Clears the queue
        /// </summary>
        public void ClearQueue() => queue.Clear();
        /// <summary>
        /// Stops and clears the queue
        /// </summary>
        public void ClearAndStop() { ClearQueue(); StopQueue(); }
        /// <summary>
        /// Clears the queue and starts it
        /// </summary>
        public void ClearAndStart() { ClearQueue(); StartQueue(); }
    }

    /// <summary>
    /// Defines data for a queued function
    /// </summary>
    /// <param name="Method"></param>
    /// <param name="Args"></param>
    public readonly struct DelegateParams(Delegate Method, params object?[]? Args)
    {
        /// <summary>
        /// Creates a Delegate of a method with no imput parameters. implicitly
        /// </summary>
        /// <param name="Method"></param>
        public static implicit operator DelegateParams(Delegate Method) => new(Method);
        /// <summary>
        /// Invokes the delegate with the arguments
        /// </summary>
        public void Invoke() => Method.DynamicInvoke(Args);
    }
}


