using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.NetworkServer.Packets.Default.Packets;
internal class DisconnectClientPacket : Packet
{
    public DisconnectClientPacket()
    {
        Header = new BasicHeader("DISCONNECTCLIENT");
        Content = new BasicContent();
    }
}
internal class ServerStoppedPacket : Packet
{
    public ServerStoppedPacket()
    {
        Header = new BasicHeader("SERVERSTOPPED");
        Content = new BasicContent();
    }
}
