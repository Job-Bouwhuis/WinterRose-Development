using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.ForgeWarden.EngineLayers.Events;

namespace WinterRose.ForgeWarden.EngineLayers;

public abstract class EngineLayer
{
    public string Name { get; }
    public bool Enabled { get; set; } = true;

    public int Importance
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            LayerStack?.MarkDirty();
        }
    }

    internal protected LayerStack LayerStack { get; internal set; }
    internal protected ForgeWardenEngine Engine => ForgeWardenEngine.Current;

    protected EngineLayer(string name)
    {
        Name = name;
    }

    public virtual void OnAttach() { }

    public virtual void OnDetach() { }

    public virtual void OnUpdate() { }

    public virtual void OnRender() { }

    public virtual void OnEvent<TEvent>(ref TEvent engineEvent)
    where TEvent : struct, IEngineEvent
    {
    }

    public void InternalUpdate()
    {
        if (!Enabled)
            return;

        OnUpdate();
    }

    public void InternalRender()
    {
        if (!Enabled)
            return;

        OnRender();
    }
}
