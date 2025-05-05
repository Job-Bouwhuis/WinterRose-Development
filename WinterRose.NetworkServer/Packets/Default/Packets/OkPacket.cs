using WinterRose.NetworkServer.Packets;
using WinterRose.NetworkServer.Packets.Default;

namespace WinterRose.NetworkServer.Packets;
internal class OkPacket : Packet
{
    public OkPacket()
    {
        Header = new BasicHeader("OK");
        Content = new BasicContent();
    }
}