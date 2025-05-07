using System;

namespace WinterRose.NetworkServer.Packets;

public class GuidContent : PacketContent
{
    public Guid guid;

    public GuidContent(Guid guid) => this.guid = guid;
    private GuidContent() { } // for serialization
}
