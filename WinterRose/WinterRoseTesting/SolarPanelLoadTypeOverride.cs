using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WinterRose;

public class SolarPanelTypeSearchOverrideCollection : IEnumerable<SolarPanelLoadTypeOverride>, ICollection<SolarPanelLoadTypeOverride>, IList<SolarPanelLoadTypeOverride>
{
    List<SolarPanelLoadTypeOverride> overrides = new();

    public int Count => overrides.Count;
    public bool IsReadOnly => false;

    public void Add(SolarPanelLoadTypeOverride typeOverride)
    {
        string identifier = FindNextAvailableName(typeOverride.Identifier);
        var toAdd = identifier == typeOverride.Identifier ? typeOverride : new(typeOverride.Type, identifier);
        overrides.Add(toAdd);
    }
    public void Clear() => overrides.Clear();
    public bool Contains(SolarPanelLoadTypeOverride typeOverride) => overrides.Contains(typeOverride);
    public void CopyTo(SolarPanelLoadTypeOverride[] array, int arrayIndex) => overrides.CopyTo(array, arrayIndex);
    public IEnumerator<SolarPanelLoadTypeOverride> GetEnumerator() => overrides.GetEnumerator();
    public int IndexOf(SolarPanelLoadTypeOverride typeOverride) => overrides.IndexOf(typeOverride);
    public void Insert(int index, SolarPanelLoadTypeOverride item) => overrides.Insert(index, item);
    public bool Remove(SolarPanelLoadTypeOverride typeOverride) => overrides.Remove(typeOverride);
    public void RemoveAt(int index) => overrides.RemoveAt(index);
    public bool Any() => overrides.Any();
    public bool Any(Func<SolarPanelLoadTypeOverride, bool> predicate) => overrides.Any(predicate);
    public void AddRange(params SolarPanelLoadTypeOverride[] typeOverrides) => overrides.AddRange(typeOverrides);

    public SolarPanelLoadTypeOverride this[int index]
    {
        get => overrides[index];
        set => overrides[index] = value;
    }

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

        return count > 1 ? $"{baseName}{nextNumber}" : baseName;
    }
    IEnumerator IEnumerable.GetEnumerator() => overrides.GetEnumerator();
}