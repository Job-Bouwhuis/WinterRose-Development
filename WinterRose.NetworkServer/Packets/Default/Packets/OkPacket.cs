using WinterRose.NetworkServer.Packets;
using WinterRose.NetworkServer.Packets.Default;

namespace WinterRose.NetworkServer.Packets;
public class OkPacket : Packet
{
    public OkPacket()
    {
        Header = new BasicHeader("OK");
        Content = new BasicContent();
    }
}