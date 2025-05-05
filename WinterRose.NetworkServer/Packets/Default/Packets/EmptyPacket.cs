using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.NetworkServer.Packets.Default.Packets
{
    class EmptyPacket : Packet
    {
        public EmptyPacket()
        {
            Header = new EmptyPacketHeader();
            Content = new EmptyPacketContent();
        }

        public class EmptyPacketHeader() : PacketHeader()
        {
            public override string GetPacketType() => "";
        }
        public class EmptyPacketContent() : PacketContent();
    }
}
