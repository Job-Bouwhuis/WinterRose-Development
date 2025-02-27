using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Serialization;

namespace WinterRose.Monogame.ModdingSystem;

/// <summary>
/// A container for mods for an object
/// </summary>
/// <typeparam name="T">The modifiable type</typeparam>
[IncludePrivateFields]
public class ModContainer<T> where T : class
{
    [Show]
    private readonly List<Mod<T>> mods = new();

    /// <summary>
    /// The total capacity of mods this container can hold
    /// </summary>
    [IncludeWithSerialization]
    public int TotalModCapacity { get; set; }

    /// <summary>
    /// Gets the current number of mod points used
    /// </summary>
    public int CurrentModPoints => mods.Sum(mod => mod.ModPoints);

    /// <summary>
    /// Adds a mod to the container if capacity allows
    /// </summary>
    /// <param name="mod">The mod to add</param>
    /// <returns>True if successfully added, false if capacity is exceeded</returns>
    public bool AddMod(Mod<T> mod)
    {
        if (CurrentModPoints + mod.ModPoints > TotalModCapacity) return false;

        mods.Add(mod);
        return true;
    }

    /// <summary>
    /// Removes a mod from the container
    /// </summary>
    public void RemoveMod(Mod<T> mod)
    {
        mods.Remove(mod);
    }

    /// <summary>
    /// Applies all mods in the container to the target object
    /// </summary>
    public void ApplyAllMods(T target)
    {
        ArgumentNullException.ThrowIfNull(target);
        foreach (var mod in mods)
            mod.Apply(target);
    }

    /// <summary>
    /// Unapplies all mods in the container from the target object
    /// </summary>
    public void UnapplyAllMods(T target)
    {
        ArgumentNullException.ThrowIfNull(target);
        foreach (var mod in mods)
            mod.Unapply(target);
    }

    /// <summary>
    /// Returns a summary of all mods currently applied
    /// </summary>
    public override string ToString()
    {
        return string.Join("\n", mods.Select(mod => mod.ToString()));
    }
}

