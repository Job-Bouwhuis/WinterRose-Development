using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WinterRose
{
    /// <summary>
    /// Provides an easy way to distribute work across multiple threads
    /// </summary>
    public sealed class WorkDistributor
    {
        /// <summary>
        /// Gets the amount of functions added to the WorkDistributor
        /// </summary>
        public int FunctionCount { get => actions.Count; }

        private readonly List<Action> actions = new();
        private int threads;

        /// <summary>
        /// Creates a new instance of the WorkDistributor class
        /// </summary>
        /// <param name="threads"></param>
        public WorkDistributor(int threads)
        {
            this.threads = threads;
        }

        /// <summary>
        /// Adds the given function with no arguments or return type to the WorkDistributor
        /// </summary>
        /// <param name="action"></param>
        public void AddFunction(Action action) => actions.Add(action);
        /// <summary>
        /// Removes all functions tied to this WorkDistributor
        /// </summary>
        public void Clear() => actions.Clear();
        /// <summary>
        /// Removes the given function from the WorkDistributor
        /// </summary>
        /// <param name="action"></param>
        public void RemoveFunction(Action action) => actions.Remove(action);

        /// <summary>
        /// Calls all the functions added to the WorkDistributor on the allowed amount of threads. <strong>this does NOT respect the order of the given functions</strong>
        /// </summary>
        /// <returns>An 32 bit signed integer conveying information on how many tasks failed</returns>
        public void Start()
        {
            if (actions.Count == 0)
                return;

            Task.WhenAll(GetTasksArray()).Wait();
        }
        /// <summary>
        /// Calls all the functions added to the WorkDistributor on the allowed amount of threads.  <strong>this does NOT respect the order of the given functions</strong>
        /// </summary>
        public void Start(int threads)
        {
            int prefThreads = this.threads;
            this.threads = threads;
            Start();
            this.threads = prefThreads;
        }
        
        /// <summary>
        /// Asynchronously calls all the functions added to the WorkDistributor on the allowed amount of threads.  <strong>this does NOT respect the order of the given functions</strong>
        /// </summary>
        public async Task StartAsync() => await Task.Run(() => Start());
        /// <summary>
        /// Asynchronously calls all the functions added to the WorkDistributor on the allowed amount of threads.  <strong>this does NOT respect the order of the given functions</strong>
        /// </summary>
        public async Task StartAsync(int threads) => await Task.Run(() => Start(threads));

        private Task[] GetTasksArray()
        {
            List<Action>[] partitions = actions.Partition(threads);
            List<Task> tasks = new();

            foreach (var part in partitions)
                tasks.Add(CreateTasks(part));

            return tasks.ToArray();
        }
        private async Task CreateTasks(List<Action> actions)
        {
            await Task.Run(() =>
            {
                foreach (Action func in actions)
                    func();
            });
        }
    }
}