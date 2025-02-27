using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Serialization;

namespace WinterRose.Monogame.ModdingSystem;
public abstract class ModAttribute<T>
{
    /// <summary>
    /// They key used for a value modifier. -1 by default
    /// </summary>
    protected int ModifierKey { get; set; } = -1;

    /// <summary>
    /// What it does expressed in text. eg: "+50% Damage"
    /// </summary>
    [IncludeWithSerialization]
    public abstract string EffectString { get; }
    /// <summary>
    /// The type of moddable this attribute is valid on
    /// </summary>
    [Hide]
    public Type Type => typeof(T);

    /// <summary>
    /// Applies this mod attribute to the given <typeparamref name="T"/> <paramref name="modifiable"/>
    /// </summary>
    /// <param name="modifiable"></param>
    public abstract void Apply(T modifiable);
    /// <summary>
    /// Unapplies the attribute that was applied to the target using <see cref="Apply(T)"/>
    /// </summary>
    /// <param name="modifiable"></param>
    public abstract void Unapply(T modifiable);
}
