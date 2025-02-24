using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame;

namespace WinterRose.Monogame
{
    /// <summary>
    /// Defines a behavior for a <see cref="WorldObject"/>. This is a abstract class that provides an update loop callback to a component class that inherits from this
    /// </summary>
    public abstract class ObjectBehavior : ObjectComponent
    {
        /// <summary>
        /// Whether this component wishes to have its <see cref="Update"/> be called in parallel with other types of this component
        /// <br></br>Can be set by decorating the component with a [<see cref="ParallelBehavior"/>] attribute
        /// </summary>
        public bool IsParallel => isParallel ??= GetType().GetCustomAttribute<ParallelBehavior>() != null;
        private bool? isParallel;

        [Show]
        private TimeSpan updateTime = TimeSpan.Zero;

        /// <summary>
        /// The time it took for the last update call to run from start to finish
        /// </summary>
        public TimeSpan UpdateTime => updateTime;

        internal void CallUpdate()
        {
            var sw = Stopwatch.StartNew();
            Update();
            sw.Stop();
            updateTime = sw.Elapsed;
        }

        protected abstract void Update();

        internal override ObjectComponent Clone(WorldObject newOwner)
        {
            ObjectComponent clone = base.Clone(newOwner);
            return clone;
        }
    }
}
