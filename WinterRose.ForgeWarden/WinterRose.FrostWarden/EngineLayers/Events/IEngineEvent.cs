using System;
using System.Collections.Generic;
using System.Text;

namespace WinterRose.ForgeWarden.EngineLayers.Events;

public interface IEngineEvent
{
    bool Handled { get; set; }
}

public struct RenderDrawEvent : IEngineEvent
{
    public bool Handled { get; set; }
}