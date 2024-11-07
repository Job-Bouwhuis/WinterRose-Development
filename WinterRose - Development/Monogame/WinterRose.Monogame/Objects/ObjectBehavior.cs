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
        [Show]
        private TimeSpan updateTime = TimeSpan.Zero;
        private MethodInfo? updateMethod;

        /// <summary>
        /// The time it took for the last update call to run from start to finish
        /// </summary>
        public TimeSpan UpdateTime => updateTime;

        public ObjectBehavior()
        {
            Initialize();
        }

        internal override void Initialize()
        {
            base.Initialize();
            string typeName = GetType().Name;

            GetUpdateMethod(GetType());

            Type t = GetType().BaseType;
            while (updateMethod is null && t != typeof(ObjectComponent) && t != typeof(object))
                t = GetUpdateMethod(t).BaseType;
        }

        Type GetUpdateMethod(Type t)
        {
            OverrideDefaultMethodNamesAttribute? attr = GetType().GetCustomAttribute<OverrideDefaultMethodNamesAttribute>();
            if (attr != null)
                updateMethod = t.GetMethod(attr.Update, MonoUtils.InstanceMemberFindingFlags);
            else
                updateMethod = t.GetMethod("Update", MonoUtils.InstanceMemberFindingFlags);

            return t;
        }

        internal void CallUpdate()
        {
            if (!initialized)
                Initialize();
            var sw = Stopwatch.StartNew();
            updateMethod?.Invoke(this, null);
            sw.Stop();
            updateTime = sw.Elapsed;
        }


        internal override ObjectComponent Clone(WorldObject newOwner)
        {
            ObjectComponent clone = base.Clone(newOwner);
            updateMethod = null;
            return clone;
        }
    }
}
