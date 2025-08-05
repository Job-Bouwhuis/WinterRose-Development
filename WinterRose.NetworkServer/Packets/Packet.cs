using System;

namespace WinterRose.NetworkServer.Packets;

public class Packet
{
    [WFInclude]
    public PacketHeader Header { get; set; }
    [WFInclude]
    public PacketContent Content { get; set; }
    [WFInclude]
    public Guid SenderID { get; set; }
    [WFInclude]
    public string SenderUsername {  get; set; }
}