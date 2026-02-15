namespace WinterRose.ForgeWarden.TextRendering;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents a modifier that can be applied to text elements.
/// Modifiers can be either persistent (replacement) or stackable (scoped).
/// </summary>
public abstract class Modifier
{
    /// <summary>
    /// Unique name of this modifier (e.g., "bold", "italic", "wave").
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Category for persistent modifiers (e.g., "color").
    /// Stackable modifiers typically have their name as category.
    /// </summary>
    public abstract string Category { get; }

    /// <summary>
    /// True if this modifier is stackable; false if it's persistent/replacement.
    /// </summary>
    public abstract bool IsStackable { get; }

    public override string ToString() => Name;
}

/// <summary>
/// Immutable snapshot of active modifiers at a point in time.
/// Captured when elements are emitted and stored on the element.
/// </summary>
public class ModifierSnapshot
{
    /// <summary>
    /// Persistent modifiers (category -> modifier).
    /// </summary>
    public IReadOnlyDictionary<string, Modifier> PersistentModifiers { get; }

    /// <summary>
    /// Stackable modifiers in order of application.
    /// </summary>
    public IReadOnlyList<Modifier> StackableModifiers { get; }

    public ModifierSnapshot(
        Dictionary<string, Modifier> persistentModifiers,
        List<Modifier> stackableModifiers)
    {
        PersistentModifiers = new Dictionary<string, Modifier>(persistentModifiers);
        StackableModifiers = new List<Modifier>(stackableModifiers);
    }

    public bool IsEmpty => PersistentModifiers.Count == 0 && StackableModifiers.Count == 0;
}

/// <summary>
/// Manages active modifiers during parsing.
/// Tracks both persistent (replacement) and stackable (scoped) modifiers.
/// </summary>
public class ModifierStack
{
    private readonly Dictionary<string, Modifier> persistentModifiers = new();
    private readonly List<Modifier> stackableModifiers = new();
    private readonly Dictionary<string, Modifier> modifierRegistry = new();

    public void RegisterModifier(Modifier modifier)
    {
        if (modifier == null)
            throw new ArgumentNullException(nameof(modifier));
        modifierRegistry[modifier.Name] = modifier;
    }

    public void PushStackable(string modifierName)
    {
        if (!modifierRegistry.TryGetValue(modifierName, out var modifier))
            return; // Silently ignore unknown modifiers

        if (!modifier.IsStackable)
            return; // Not a stackable modifier

        stackableModifiers.Add(modifier);
        snapshotDirty = true;
    }

    public bool PopStackable(string modifierName)
    {
        // Remove the most recent modifier with this name
        for (int i = stackableModifiers.Count - 1; i >= 0; i--)
        {
            if (stackableModifiers[i].Name == modifierName)
            {
                stackableModifiers.RemoveAt(i);
                snapshotDirty = true;
                return true;
            }
        }
        return false; // No matching modifier found
    }

    public void SetPersistent(string modifierName, Modifier modifier)
    {
        if (modifier == null)
        {
            persistentModifiers.Remove(modifierName);
            return;
        }

        if (!modifierRegistry.ContainsKey(modifierName))
            RegisterModifier(modifier);

        persistentModifiers[modifier.Category] = modifier;
        snapshotDirty = true;
    }

    public Modifier GetPersistent(string category)
    {
        persistentModifiers.TryGetValue(category, out var modifier);
        return modifier;
    }

    private ModifierSnapshot cachedSnapshot;
    private bool snapshotDirty = true;

    public ModifierSnapshot GetSnapshot()
    {
        if (snapshotDirty || cachedSnapshot == null)
        {
            cachedSnapshot = BuildSnapshot();
            snapshotDirty = false;
        }

        return cachedSnapshot;
    }


    private ModifierSnapshot BuildSnapshot()
    {
        return new ModifierSnapshot(persistentModifiers, stackableModifiers);
    }

    public void Clear()
    {
        persistentModifiers.Clear();
        stackableModifiers.Clear();
    }
}
