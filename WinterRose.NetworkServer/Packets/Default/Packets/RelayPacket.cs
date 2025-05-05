using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.NetworkServer.Packets.Default.Packets
{
    class RelayPacket : Packet
    {
        public RelayPacket(Packet relayed, Guid sender, Guid destination)
        {
            Header = new BasicHeader("RELAYPACKET");
            Content = new RelayContent(relayed, sender, destination);
        }

        public class RelayContent(Packet relayed, Guid sender, Guid dest) : PacketContent
        {
            public Packet relayedPacket = relayed;
            public Guid destination = dest;
            public Guid sender = sender;
        }
    }
}
