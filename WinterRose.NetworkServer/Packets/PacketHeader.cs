﻿
using System;

namespace WinterRose.NetworkServer.Packets;

// Abstract header
public abstract class PacketHeader
{
    [IncludeWithSerialization]
    public Guid CorrelationId { get; internal set; }

    public abstract string GetPacketType();
}
