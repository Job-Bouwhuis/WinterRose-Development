using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace WinterRose
{
    /// <summary>
    /// Used to report the progress of something
    /// </summary>
    public class ProgressReporter
    {
        /// <summary>
        /// the percentage of completion
        /// </summary>
        public float Progress { get; private set; }
        /// <summary>
        /// a descriptive message 
        /// </summary>
        public string Message { get; private set; }

        internal ProgressReporter(float progress, string message)
        {
            Progress = progress;
            Message = message;
        }
        /// <summary>
        /// Create your own process reporting object.
        /// </summary>
        public ProgressReporter()
        {
            Progress = 0;
            Message = "No process has started...";
        }

        /// <summary>
        /// Gets the message of progress reporter
        /// </summary>
        /// <param name="e"></param>
        public static implicit operator string(ProgressReporter e) => e.Message;
        /// <summary>
        /// get the persentage value of the progress reporter
        /// </summary>
        /// <param name="e"></param>
        public static implicit operator float(ProgressReporter e) => e.Progress;

        /// <summary>
        /// Sets the message for this object.
        /// </summary>
        /// <param name="message"></param>
        public void SetMessage(string message) => Message = message;
        /// <summary>
        /// Sets the progress of this object. Value is expected to be a persentage
        /// </summary>
        /// <param name="progress"></param>
        public void SetProgress(float progress) => Progress = progress;
    }
}

namespace WinterRose.Serialization
{
    [IncludePrivateFields]
    internal class EventMethodInfo
    {
        internal string typeName;
        internal string typeAssembly;
        internal string methodName;

        public EventMethodInfo()
        {
            Type t = typeof(EventMethodInfo);
            typeName = t.Name;
            typeAssembly = t.Assembly.FullName?.Base64Encode() ?? "no assembly name";
            methodName = "404NoMethodAttached";
        }
        public EventMethodInfo(Type type, string methodName)
        {
            Type t = type;
            typeName = t.Name;
            typeAssembly = t.Assembly.FullName?.Base64Encode() ?? "no assembly name";
            this.methodName = methodName;
        }
    }
    internal class EventHelper<TClass, TDelegate> where TDelegate : Delegate
    {

        public EventHelper(string eventName)
        {
            var fieldInfo = typeof(TClass).GetField(eventName, BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance) ??
        throw new ArgumentException("Event was not found", nameof(eventName));

            var thisArg = Expression.Parameter(typeof(TClass));
            var body = Expression.Convert(Expression.Field(thisArg, fieldInfo), typeof(TDelegate));
            Get = Expression.Lambda<Func<TClass, TDelegate>>(body, thisArg).Compile();

        }

        public EventHelper(string eventName, TClass c, TDelegate d)
        {
            Type t1 = typeof(TClass);
            Type t2 = typeof(TDelegate);
            Type tt1 = c.GetType();
            Type tt2 = d.GetType();

            var fieldInfo = tt1.GetField(eventName, BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance) ??
        throw new ArgumentException("Event was not found", nameof(eventName));

            var thisArg = Expression.Parameter(tt1);
            var body = Expression.Convert(Expression.Field(thisArg, fieldInfo), typeof(TDelegate));
            Get = Expression.Lambda<Func<TClass, TDelegate>>(body, thisArg).Compile();

        }


        private Func<TClass, TDelegate> Get { get; set; }

        public IEnumerable<TDelegate> GetInvocationList(TClass c)
        {
            var eventDelegate = Get(c);
            if (eventDelegate is null)
                yield break;

            foreach (var d in eventDelegate.GetInvocationList())
                yield return (TDelegate)d;
        }
    }
}
