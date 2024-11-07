using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WinterRose.Monogame.Worlds;

/// <summary>
/// A collection of <see cref="WorldTemplateTypeSearchOverride"/>s.
/// </summary>
public class WorldTemplateTypeSearchOverrideCollection : IEnumerable<WorldTemplateTypeSearchOverride>, ICollection<WorldTemplateTypeSearchOverride>, IList<WorldTemplateTypeSearchOverride>
{
    List<WorldTemplateTypeSearchOverride> overrides = new();

    public WorldTemplateTypeSearchOverride this[int index] { get => overrides[index]; set => overrides[index] = value; }

    public int Count => overrides.Count;
    public bool IsReadOnly => false;

    /// <summary>
    /// Adds the <paramref name="typeOverride"/> to the collection.
    /// </summary>
    /// <param name="typeOverride"></param>
    public void Add(WorldTemplateTypeSearchOverride typeOverride)
    {
        string identifier = FindNextAvailableName(typeOverride.Identifier);
        var toAdd = identifier == typeOverride.Identifier ? typeOverride : new(typeOverride.Type, identifier);
        overrides.Add(toAdd);
    }
    /// <summary>
    /// Clears the collection
    /// </summary>
    public void Clear() => overrides.Clear();
    /// <summary>
    /// Checks whether the collection has the specified <paramref name="typeOverride"/>.
    /// </summary>
    /// <param name="typeOverride"></param>
    /// <returns></returns>
    public bool Contains(WorldTemplateTypeSearchOverride typeOverride) => overrides.Contains(typeOverride);
    /// <summary>
    /// Copies the collection to the specified <paramref name="array"/> starting at the specified <paramref name="arrayIndex"/>.
    /// </summary>
    /// <param name="array"></param>
    /// <param name="arrayIndex"></param>
    public void CopyTo(WorldTemplateTypeSearchOverride[] array, int arrayIndex) => overrides.CopyTo(array, arrayIndex);
    /// <summary>
    /// Gets the enumerator for the collection.
    /// </summary>
    /// <returns></returns>
    public IEnumerator<WorldTemplateTypeSearchOverride> GetEnumerator() => overrides.GetEnumerator();
    /// <summary>
    /// Gets the index of the specified <paramref name="typeOverride"/>.
    /// </summary>
    /// <param name="typeOverride"></param>
    /// <returns></returns>
    public int IndexOf(WorldTemplateTypeSearchOverride typeOverride) => overrides.IndexOf(typeOverride);
    /// <summary>
    /// Inserts the specified <paramref name="item"/> at the specified <paramref name="index"/>.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="item"></param>
    public void Insert(int index, WorldTemplateTypeSearchOverride item) => overrides.Insert(index, item);
    /// <summary>
    /// Removes the specified <paramref name="typeOverride"/> from the collection.
    /// </summary>
    /// <param name="typeOverride"></param>
    /// <returns></returns>
    public bool Remove(WorldTemplateTypeSearchOverride typeOverride) => overrides.Remove(typeOverride);
    /// <summary>
    /// Removes the item at the specified <paramref name="index"/>.
    /// </summary>
    /// <param name="index"></param>
    public void RemoveAt(int index) => overrides.RemoveAt(index);
    /// <summary>
    /// Checks whether the collection has any items.
    /// </summary>
    /// <returns></returns>
    public bool Any() => overrides.Any();
    /// <summary>
    /// Checks whether the collection has any items matching the specified <paramref name="predicate"/>.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public bool Any(Func<WorldTemplateTypeSearchOverride, bool> predicate) => overrides.Any(predicate);
    /// <summary>
    /// Checks whether the collection has any items matching the specified <paramref name="predicate"/>.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public bool Any(WorldTemplateTypeSearchOverride predicate) => overrides.Any(predicate);
    /// <summary>
    /// Adds the specified <paramref name="typeOverrides"/> to the collection.
    /// </summary>
    /// <param name="typeOverrides"></param>
    public void AddRange(params WorldTemplateTypeSearchOverride[] typeOverrides) => overrides.AddRange(typeOverrides);

    private string FindNextAvailableName(string baseName)
    {
        // Count the number of components with the exact same identifier
        int count = overrides.Count(component => component.Identifier == baseName);

        // Find all matching components in the list
        var matchingComponents = overrides
            .Where(component => component.Identifier == baseName)
            .ToList();

        if (count == 0)
        {
            return baseName;
        }

        // Create a regular expression pattern to match the base name + any number
        string pattern = $@"^{Regex.Escape(baseName)}(\d+)?$";
        Regex regex = new Regex(pattern);

        // Find all matching component names in the list
        var matches = matchingComponents
            .Where(component => regex.IsMatch(component.Identifier))
            .Select(component => int.TryParse(regex.Match(component.Identifier).Groups[1].Value, out int number) ? number : 0)
            .ToList();

        // Find the first available number
        int nextNumber = 1;
        while (matches.Contains(nextNumber))
        {
            nextNumber++;
        }

        return $"{baseName}__{nextNumber}";
    }
    IEnumerator IEnumerable.GetEnumerator() => overrides.GetEnumerator();

    internal string? GetDefinition(Type type, object obj)
    {
        var types = overrides.Where(x => x.Type == type);
        foreach(var def in types)
        {
            if (!def.HasCustomParser)
                continue;
            return def.GetParsedString(obj);
        }
        return null;
    }
}