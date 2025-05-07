using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WinterRose.NetworkServer.Packets.TunnelRequestPacket;

namespace WinterRose.NetworkServer.Packets
{
    public class TunnelAcceptedPacket : Packet
    {
        internal TunnelAcceptedPacket()
        {
            Header = new BasicHeader("TUNNELACCEPTED");
            Content = new TunnelReqContent();
        }
    }
}
