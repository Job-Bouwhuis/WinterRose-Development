using System;
using WinterRose.Monogame.Internals;

namespace WinterRose.Monogame
{
    /// <summary>
    /// Tells the engine that this <see cref="ObjectBehavior"/> or <see cref="ObjectComponent"/> depends on the component/behavior of type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class RequireComponentAttribute<T> : RequireComponentAttribute
    {
        /// <summary>
        /// The component that is required
        /// </summary>
        public override Type ComponentType => typeof(T);
    }
}

namespace WinterRose.Monogame.Internals
{
    /// <summary>
    /// Useless on its own. see <see cref="RequireComponentAttribute{T}"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class RequireComponentAttribute : Attribute
    {
        public abstract Type ComponentType { get; }
        /// <summary>
        /// The arguments used when automatically creating the component if it is not yet on the object
        /// </summary>
        public object[] DefaultArguments { get; set; } = [];

        /// <summary>
        /// Whether or not to automatically add the component
        /// </summary>
        public bool AutoAdd { get; set; } = false;
    }
}


