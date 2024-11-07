using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.Attributes;

/// <summary>
/// Signals the object that no more than <see cref="limit"/> of the component may be added to a single object
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
internal sealed class ComponentLimitAttribute : Attribute
{
    internal int limit;

    /// <summary>
    /// Creates a new instance of this attribute
    /// </summary>
    /// <param name="limit">the amount of allowed components of this type on a single <see cref="WorldObject"/></param>
    public ComponentLimitAttribute(int limit) => this.limit = limit;
    /// <summary>
    /// Creates a new instance of this attribute where the limit is set to 1
    /// </summary>
    public ComponentLimitAttribute() : this(1) { }
}
