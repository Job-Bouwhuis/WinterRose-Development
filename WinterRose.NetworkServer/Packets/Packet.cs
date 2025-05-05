namespace WinterRose.NetworkServer.Packets;

public abstract class Packet
{
    [IncludeWithSerialization]
    public PacketHeader Header { get; set; }
    [IncludeWithSerialization]
    public PacketContent Content { get; set; }
    public NetworkConnection Source { get; set; }
}