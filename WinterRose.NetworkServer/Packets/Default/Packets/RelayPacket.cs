using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.NetworkServer.Packets
{
    class RelayPacket : Packet
    {
        private RelayPacket() { } // for serialization

        public RelayPacket(Packet relayed, Guid sender, Guid destination)
        {
            Header = new BasicHeader("RELAYPACKET");
            Content = new RelayContent(relayed, sender, destination);
        }

        public class RelayContent : PacketContent
        {

            public Packet relayedPacket;
            public Guid destination;
            public Guid sender;

            private RelayContent() { } // for serialization
            public RelayContent(Packet relayed, Guid sender, Guid dest)
            {
                relayedPacket = relayed;
                destination = dest;
                this.sender = sender;
            }
        }
    }
}
