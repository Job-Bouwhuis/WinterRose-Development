using System;

namespace WinterRose.NetworkServer.Packets;

public class Packet
{
    [IncludeWithSerialization]
    public PacketHeader Header { get; set; }
    [IncludeWithSerialization]
    public PacketContent Content { get; set; }
    [IncludeWithSerialization]
    public Guid SenderID { get; set; }
    [IncludeWithSerialization]
    public string SenderUsername {  get; set; }
}