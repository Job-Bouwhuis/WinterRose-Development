using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinterRose
{
    /// <summary>
    /// Allows for queueing of Methods with no arguments and no return values.
    /// </summary>
    public class ActionQueuer
    {
        private readonly List<Action> queue;
        /// <summary>
        /// Gets the number of methods in the queue
        /// </summary>
        public int Count => queue.Count;
        /// <summary>
        /// Creates a new instance of the ActionQueuer class
        /// </summary>
        public ActionQueuer() => queue = new List<Action>();

        /// <summary>
        /// Adds a new action to the queue
        /// </summary>
        /// <param name="action">The action that will be added</param>
        public void Add(Action action) => queue.Add(action);

        /// <summary>
        /// Starts the queue
        /// </summary>
        /// <returns>The completed task</returns>
        public async Task StartQueue() => await Queue();
        private async Task<bool> Queue()
        {
            if (queue.Count > 0)
            {
                var temp = queue;
                while (temp.Count > 0)
                {
                    var action = temp[0];
                    await Task.Run(() => action());
                    temp.Remove(action);
                    queue.Remove(action);
                }
            }
            return true;
        }
    }
    /// <summary>
    /// allows for queueing of Methods with one argument and no return values.
    /// </summary>
    /// <typeparam name="T1">Argument 1</typeparam>
    public class ActionQueuer<T1>
    {
        private readonly Dictionary<int, QueueArgumentCarrier> queue;

        /// <summary>
        /// Creates a new instance of the ActionQueuer class
        /// </summary>
        public ActionQueuer() => queue = new Dictionary<int, QueueArgumentCarrier>();

        /// <summary>
        /// Gets the number of methods in the queue
        /// </summary>
        public int Count => queue.Count;

        /// <summary>
        /// Adds a new action to the queue
        /// </summary>
        /// <param name="argument">The action that will be added, alongside its argument</param>
        public void Add(QueueArgumentCarrier argument) => queue.Add(queue.NextAvalible(), argument);
        /// <summary>
        /// Starts the queue
        /// </summary>
        /// <returns>The completed task</returns>
        public async Task StartQueue() => await Queue();
        private async Task<bool> Queue()
        {
            if (queue.Count > 0)
            {
                var temp = queue;
                while (temp.Count > 0)
                {
                    KeyValuePair<int, QueueArgumentCarrier> pair = temp.First();
                    int index = pair.Key;
                    var action = pair.Value.action;
                    await Task.Run(() => action(pair.Value.argumentValue));
                    temp.Remove(index);
                    queue.Remove(index);
                }
            }
            return true;
        }
        /// <summary>
        /// allows for the carrying of arguments to actions in the queue
        /// </summary>
        public class QueueArgumentCarrier
        {
            internal Action<T1> action;
            internal T1 argumentValue;
            /// <summary>
            /// Creates a new instance of the <b>QueueArgumentCarrier</b> class
            /// </summary>
            /// <param name="action"></param>
            /// <param name="argument"></param>
            public QueueArgumentCarrier(Action<T1> action, T1 argument)
            {
                argumentValue = argument;
                this.action = action;
            }
            /// <summary>
            /// Gets the arguments value
            /// </summary>
            /// <param name="argument"></param>
            public static implicit operator T1(QueueArgumentCarrier argument) => argument.argumentValue;

        }
    }
    /// <summary>
    /// allows for queueing of Methods with two arguments, and no return types. Contact the author of this library if you wish to use more arguments
    /// </summary>
    /// <typeparam name="T1">argument 1</typeparam>
    /// <typeparam name="T2">argument 2</typeparam>
    public class ActionQueuer<T1, T2>
    {
        private readonly Dictionary<int, QueueArgumentCarrier> queue;

        /// <summary>
        /// Gets the number of methods in the queue
        /// </summary>
        public int Count => queue.Count;

        /// <summary>
        /// Creates a new instance of the ActionQueuer class
        /// </summary>
        public ActionQueuer() => queue = new Dictionary<int, QueueArgumentCarrier>();

        /// <summary>
        /// Adds a new action to the queue
        /// </summary>
        /// <param name="arguments">The action that will be added, alongside its argument</param>
        public void Add(QueueArgumentCarrier arguments) => queue.Add(queue.NextAvalible(), arguments);

        /// <summary>
        /// Starts the queue
        /// </summary>
        /// <returns>The completed task</returns>
        public async Task StartQueue() => await Queue();
        private async Task<bool> Queue()
        {
            if (queue.Count > 0)
            {
                var temp = queue;
                while (temp.Count > 0)
                {
                    var pair = temp.First();
                    int index = pair.Key;
                    var action = pair.Value.action;
                    await Task.Run(() => action(pair.Value.argument1Value, pair.Value.argument2Value));
                    temp.Remove(index);
                    queue.Remove(index);
                }
            }
            return true;
        }
        /// <summary>
        /// Allows for the carrying of a Action with arguments
        /// </summary>
        public class QueueArgumentCarrier
        {
            internal Action<T1, T2> action;
            internal T1 argument1Value;
            internal T2 argument2Value;

            /// <summary>
            /// allows for the carrying of arguments to actions in the queue
            /// </summary>
            public QueueArgumentCarrier(Action<T1, T2> action, T1 argument1, T2 argument2)
            {
                this.action = action;
                argument1Value = argument1;
                argument2Value = argument2;
            }
            /// <summary>
            /// Gets the first argument
            /// </summary>
            /// <param name="argument"></param>
            public static implicit operator T1(QueueArgumentCarrier argument) => argument.argument1Value;
            /// <summary>
            /// Gets the second argument
            /// </summary>
            /// <param name="argument"></param>
            public static implicit operator T2(QueueArgumentCarrier argument) => argument.argument2Value;
        }
    }

    /// <summary>
    /// allows for queueing of Methods with no arguments and a return value
    /// </summary>
    /// <typeparam name="TReturnType">Return value</typeparam>
    public class FuncQueuer<TReturnType>
    {
        private readonly Dictionary<int, QueueFuncArgumentCarrier> queue;

        /// <summary>
        /// Gets the number of methods in the queue
        /// </summary>
        public int Count => queue.Count;
        /// <summary>
        /// Creates a new instance of the FuncQueuer class
        /// </summary>
        public FuncQueuer() => queue = new Dictionary<int, QueueFuncArgumentCarrier>();

        /// <summary>
        /// Adds the given Func to the queue
        /// </summary>
        /// <param name="arguments">the func that will be added, along side its callback action</param>
        public void Add(QueueFuncArgumentCarrier arguments) => queue.Add(queue.NextAvalible(), arguments);
        /// <summary>
        /// Starts the queue
        /// </summary>
        /// <param name="waitForCallback"></param>
        /// <returns>The completed task</returns>
        public async Task StartQueue(bool waitForCallback) => await Queue(waitForCallback);
        private async Task<bool> Queue(bool waitForCallback)
        {
            if (queue.Count > 0)
            {
                var temp = queue;
                while (temp.Count > 0)
                {
                    var pair = temp.First();
                    Func<TReturnType> func = pair.Value.func;
                    await Task.Run(async () =>
                    {
                        TReturnType returned = func();
                        if (waitForCallback)
                            await CallBack(pair.Value, returned);
                        else
                            _ = CallBack(pair.Value, returned);
                    });
                    temp.Remove(pair.Key);
                    queue.Remove(pair.Key);
                }
            }
            return true;
        }
        private static async Task CallBack(QueueFuncArgumentCarrier callback, TReturnType returnValue) => await Task.Run(() => callback.callBack(returnValue));

        /// <summary>
        /// Allows for the carrying of a func with a callback action and arguments
        /// </summary>
        public class QueueFuncArgumentCarrier
        {
            internal Func<TReturnType> func;
            internal Action<TReturnType> callBack;
            /// <summary>
            /// Creates a new instance of the <b>QueueFuncArgumentCarrier</b> class
            /// </summary>
            /// <param name="func"></param>
            /// <param name="callBack"></param>
            public QueueFuncArgumentCarrier(Func<TReturnType> func, Action<TReturnType> callBack)
            {
                this.callBack = callBack;
                this.func = func;
            }
        }
    }
    /// <summary>
    /// allows for queueing of Methods with one argument and a return value.
    /// </summary>
    /// <typeparam name="TArgument1">argument 1</typeparam>
    /// <typeparam name="TReturnType">return value</typeparam>
    public class FuncQueuer<TArgument1, TReturnType>
    {
        private readonly Dictionary<int, QueueFuncArgumentCarrier> queue;
        /// <summary>
        /// Gets the number of methods currently in the queue
        /// </summary>
        public int Count => queue.Count;
        public FuncQueuer() => queue = new Dictionary<int, QueueFuncArgumentCarrier>();

        /// <summary>
        /// adds the specified func with its callback action and its argument to the queue
        /// </summary>
        /// <param name="arguments"></param>
        public void Add(QueueFuncArgumentCarrier arguments) => queue.Add(queue.NextAvalible(), arguments);
        /// <summary>
        /// Starts the queue
        /// </summary>
        /// <returns>The Completed Task</returns>
        public async Task StartQueue() => await Queue();
        private async Task<bool> Queue()
        {
            if (queue.Count > 0)
            {
                var temp = queue;
                while (temp.Count > 0)
                {
                    var pair = temp.First();
                    var func = pair.Value.func;
                    await Task.Run(() =>
                    {
                        TReturnType returned = func(pair.Value.argument1);
                        _ = CallBack(pair.Value, returned);
                    });
                    temp.Remove(pair.Key);
                    queue.Remove(pair.Key);
                }
            }
            return true;
        }
        private static async Task CallBack(QueueFuncArgumentCarrier callback, TReturnType returnValue) => await Task.Run(() => callback.callBack(returnValue));

        /// <summary>
        /// Allows for the carrying of a func with a callback action and arguments
        /// </summary>
        public class QueueFuncArgumentCarrier
        {
            internal Func<TArgument1, TReturnType> func;
            internal Action<TReturnType> callBack;
            internal TArgument1 argument1;
            /// <summary>
            /// Creates a new instance of the <b>QueueFuncArgumentCarrier</b> class
            /// </summary>
            /// <param name="func"></param>
            /// <param name="callBack"></param>
            public QueueFuncArgumentCarrier(Func<TArgument1, TReturnType> func, TArgument1 argument1, Action<TReturnType> callBack)
            {
                this.callBack = callBack;
                this.func = func;
                this.argument1 = argument1;
            }
            /// <summary>
            /// Gets the argument
            /// </summary>
            /// <param name="argument"></param>
            public static implicit operator TArgument1(QueueFuncArgumentCarrier argument) => argument.argument1;
        }
    }
    /// <summary>
    /// Contact the author of this library if you wish to use more arguments
    /// </summary>
    /// <typeparam name="TArgument1">argument 1</typeparam>
    /// <typeparam name="TArgument2">argument 2</typeparam>
    /// <typeparam name="TReturnType">return value</typeparam>
    public class FuncQueuer<TArgument1, TArgument2, TReturnType>
    {
        private readonly Dictionary<int, QueueFuncArgumentCarrier> queue;
        /// <summary>
        /// Gets the number of methods currently in the queue
        /// </summary>
        public int Count => queue.Count;
        /// <summary>
        /// Creates a new instance of the FuncQueuer class
        /// </summary>
        public FuncQueuer() => queue = new Dictionary<int, QueueFuncArgumentCarrier>();

        /// <summary>
        /// Adds the given func with its callback action and arguments to the queue
        /// </summary>
        /// <param name="arguments"></param>
        public void Add(QueueFuncArgumentCarrier arguments) => queue.Add(queue.NextAvalible(), arguments);
        /// <summary>
        /// Starts the queue
        /// </summary>
        /// <returns>The completed task</returns>
        public async Task StartQueue() => await Queue();
        private async Task<bool> Queue()
        {
            if (queue.Count > 0)
            {
                var temp = queue;
                while (temp.Count > 0)
                {
                    var pair = temp.First();
                    var func = pair.Value.func;
                    await Task.Run(() =>
                    {
                        TReturnType returned = func(pair.Value.argument1, pair.Value.argument2);
                        _ = CallBack(pair.Value, returned);
                    });
                    temp.Remove(pair.Key);
                    queue.Remove(pair.Key);
                }
            }
            return true;
        }
        private static async Task CallBack(QueueFuncArgumentCarrier callback, TReturnType returnValue) => await Task.Run(() => callback.callBack(returnValue));

        /// <summary>
        /// Allows for the carrying of a func with a callback action and arguments
        /// </summary>
        public struct QueueFuncArgumentCarrier
        {
            internal Func<TArgument1, TArgument2, TReturnType> func;
            internal Action<TReturnType> callBack;
            internal TArgument1 argument1;
            internal TArgument2 argument2;
            /// <summary>
            /// Creates a new instance of the FuncQueuer class
            /// </summary>
            public QueueFuncArgumentCarrier(Func<TArgument1, TArgument2, TReturnType> func, TArgument1 argument1, TArgument2 argument2, Action<TReturnType> callBack)
            {
                this.callBack = callBack;
                this.func = func;
                this.argument1 = argument1;
                this.argument2 = argument2;
            }
            /// <summary>
            /// Gets the first argument
            /// </summary>
            /// <param name="argument"></param>
            public static implicit operator TArgument1(QueueFuncArgumentCarrier argument) => argument.argument1;
            /// <summary>
            /// Gets the second argument
            /// </summary>
            /// <param name="argument"></param>
            public static implicit operator TArgument2(QueueFuncArgumentCarrier argument) => argument.argument2;
        }
    }
}



