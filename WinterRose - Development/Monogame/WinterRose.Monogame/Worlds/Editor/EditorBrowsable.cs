using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.Worlds;

/// <summary>
/// States that the template is browsable in the editor. Only used for the <see cref="WorldTemplate"/> class
/// </summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
public class EditorBrowsableAttribute : Attribute
{
    /// <summary>
    /// The state of the editor browsable attribute
    /// </summary>
    public readonly EditorBrowsableState state;
    /// <summary>
    /// Creates a new instance of the <see cref="EditorBrowsableAttribute"/> class
    /// </summary>
    /// <param name="state">The state of the editor browsable attribute</param>
    public EditorBrowsableAttribute(EditorBrowsableState state)
    {
        this.state = state;
    }
}

/// <summary>
/// The state of the editor browsable attribute
/// </summary>
public enum EditorBrowsableState
{
    /// <summary>
    /// The item is always browsable in the editor (default)
    /// </summary>
    Always,
    /// <summary>
    /// The item is only browsable in the editor when the project is in debug mode
    /// </summary>
    Debug,
    /// <summary>
    /// The item is never browsable in the editor
    /// </summary>
    Never
}
