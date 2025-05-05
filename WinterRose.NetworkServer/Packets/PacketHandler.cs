using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.NetworkServer.Packets
{
    public abstract class PacketHandler
    {
        public abstract string Type { get; }

        public abstract void Handle(Packet packet, NetworkConnection self, NetworkConnection sender);

        // New abstract method to handle response packets
        public abstract void HandleResponsePacket(ReplyPacket replyPacket, Packet packet, NetworkConnection self, NetworkConnection sender);

        protected static PacketHandler? GetHandler(Packet packet) => NetworkConnection.GetHandler(packet);
    }
}
