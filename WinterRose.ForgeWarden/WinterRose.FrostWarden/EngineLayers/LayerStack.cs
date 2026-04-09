using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.ForgeWarden.EngineLayers.Events;

namespace WinterRose.ForgeWarden.EngineLayers;

public sealed class LayerStack
{
    private readonly List<EngineLayer> layers = new();
    private bool dirty;

    public IReadOnlyList<EngineLayer> Layers => layers;

    public void AddLayer(EngineLayer layer)
    {
        layers.Add(layer);
        layer.LayerStack = this;
        layer.OnAttach();

        dirty = true;
    }

    public void RemoveLayer(EngineLayer layer)
    {
        if (layers.Remove(layer))
        {
            layer.OnDetach();
            layer.LayerStack = null;
            dirty = true;
        }
    }

    internal void MarkDirty()
    {
        dirty = true;
    }

    private void SortIfNeeded()
    {
        if (!dirty)
            return;

        layers.Sort((a, b) => a.Importance.CompareTo(b.Importance));
        dirty = false;
    }

    public void Update()
    {
        SortIfNeeded();

        for (int i = 0; i < layers.Count; i++)
        {
            EngineLayer layer = layers[i];

            if (!layer.Enabled)
                continue;

            layer.OnUpdate();
        }
    }

    public void Render()
    {
        SortIfNeeded();

        for (int i = 0; i < layers.Count; i++)
        {
            EngineLayer layer = layers[i];

            if (!layer.Enabled)
                continue;

            layer.OnRender();
        }
    }

    public void Dispatch<TEvent>(ref TEvent engineEvent)
        where TEvent : struct, IEngineEvent
    {
        SortIfNeeded();

        for (int i = 0; i < layers.Count; i++)
        {
            EngineLayer layer = layers[i];

            if (!layer.Enabled)
                continue;

            layer.OnEvent(ref engineEvent);

            if (engineEvent.Handled)
                return;
        }
    }

    public void DispatchReverse<TEvent>(ref TEvent engineEvent)
        where TEvent : struct, IEngineEvent
    {
        SortIfNeeded();

        for (int i = layers.Count - 1; i >= 0; i--)
        {
            EngineLayer layer = layers[i];

            if (!layer.Enabled)
                continue;

            layer.OnEvent(ref engineEvent);

            if (engineEvent.Handled)
                return;
        }
    }
}
