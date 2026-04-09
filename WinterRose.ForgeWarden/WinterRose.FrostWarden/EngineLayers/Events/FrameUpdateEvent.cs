using System;
using System.Collections.Generic;
using System.Text;

namespace WinterRose.ForgeWarden.EngineLayers.Events;

public struct FrameUpdateEvent : IEngineEvent
{
    public bool Handled { get; set; }

    public Camera? Camera;
}
