using System;
using System.Net;

namespace WinterRose.NetworkServer.Packets;

public class Packet
{
    public PacketHeader Header { get; set; }
    public IPAddress SenderIP { get; set; }
    public PacketContent Content { get; set; }
    public Guid SenderID { get; set; }
    public string SenderUsername {  get; set; }
}