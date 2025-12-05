using WinterRose.ForgeMantle.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WinterRose.ForgeMantle.Models;

namespace WinterRose.ForgeMantle;
public class ConfigManager
{
    private readonly List<ConfigLayer> layers = new();
    private readonly LinkedList<List<ConfigLayer>> undoBuffer = new();
    private readonly int undoLimit;

    public ConfigManager(int undoLimit = 10)
    {
        this.undoLimit = undoLimit;
    }

    public void AddLayer(ConfigLayer layer)
    {
        layers.Add(layer);
    }

    public void Update(Action<ConfigSnapshot> updates)
    {
        var writableLayer = layers.LastOrDefault(l => l.IsWritable);
        if (writableLayer == null)
            throw new InvalidOperationException("No writable layer found.");

        writableLayer.Stage(updates);
    }

    public void ApplyChanges()
    {
        SaveUndo();

        foreach (var layer in layers)
            layer.Apply();
    }

    public void Undo()
    {
        if (undoBuffer.Count == 0)
            return;

        var last = undoBuffer.Last!.Value;
        undoBuffer.RemoveLast();

        layers.Clear();
        layers.AddRange(last);
    }

    public object? GetValue(string key)
    {
        for (int i = layers.Count - 1; i >= 0; i--)
        {
            var value = layers[i].TryGet(key);
            if (value.Found)
                return value.Value;
        }
        return null;
    }

    private void SaveUndo()
    {
        // Enforce limit
        if (undoBuffer.Count >= undoLimit)
            undoBuffer.RemoveFirst();

        undoBuffer.AddLast(layers.Select(l => l.Clone()).ToList());
    }

    public void Save()
    {
        foreach(var layer in layers)
        {
            layer.Save();
        }
    }

    public void Restore()
    {
        foreach (var layer in layers)
        {
            layer.Restore();
        }
    }
}
