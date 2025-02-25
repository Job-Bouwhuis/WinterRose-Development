using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.ModdingSystem;

public class Mod<T> where T : class
{
    [Show]
    private readonly List<ModAttribute<T>> attributes = new();

    public Mod(string name, string description)
    {
        Name = name;
        Description = description;
    }

    /// <summary>
    /// Name of the mod, e.g., "Fortified"
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// A description of what the mod does
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The number of mod points this mod costs (for balance or inventory systems)
    /// </summary>
    public int ModPoints { get; set; }

    /// <summary>
    /// Adds an attribute to this mod
    /// </summary>
    public void AddAttribute(ModAttribute<T> attribute)
    {
        ArgumentNullException.ThrowIfNull(attribute);
        attributes.Add(attribute);
    }

    public void AddAttribute<TAttribute>() where TAttribute : ModAttribute<T>
    {
        var instance = ActivatorExtra.CreateInstance<TAttribute>();
        AddAttribute(instance);
    }

    /// <summary>
    /// Removes an attribute from this mod
    /// </summary>
    public void RemoveAttribute(ModAttribute<T> attribute)
    {
        attributes.Remove(attribute);
    }

    /// <summary>
    /// Applies all mod attributes to the given target
    /// </summary>
    public void Apply(T target)
    {
        ArgumentNullException.ThrowIfNull(target);
        foreach (var attribute in attributes)
            attribute.Apply(target);
    }

    /// <summary>
    /// Removes all mod attributes from the given target
    /// </summary>
    public void Unapply(T target)
    {
        ArgumentNullException.ThrowIfNull(target);
        foreach (var attribute in attributes)
            attribute.Unapply(target);
    }

    /// <summary>
    /// Returns a summary of the mod, including its name and effect descriptions
    /// </summary>
    public override string ToString()
    {
        var effectSummary = string.Join(",\n", attributes.Select(a => a.EffectString));
        return $"{Name} ({ModPoints} Points): {effectSummary}";
    }
}

