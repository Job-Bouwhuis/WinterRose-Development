using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.Worlds;

/// <summary>
/// A class that represents a world template. <br></br>
/// Should have a <see cref="WorldTemplateAttribute"/> attached to it to be recognized as a world template
/// </summary>
public abstract class WorldTemplate
{
    /// <summary>
    /// The name of the world that will be created from this template
    /// </summary>
    public string Name { get; set; } = "Unnamed World";
    /// <summary>
    /// When overridden in a derived class, this method will be called when the world should be built and returned
    /// </summary>
    public abstract void Build(in World world);
}
