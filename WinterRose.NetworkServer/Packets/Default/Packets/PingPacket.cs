using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.NetworkServer.Packets
{
    public class PingPacket : Packet
    {
        public PingPacket()
        {
            Header = new PingHeader();
            Content = new PingContent { timestamp = DateTime.UtcNow.Ticks };
        }

        public class PingHeader : PacketHeader
        {
            public override string GetPacketType() => "ping";
        }

        public class PingContent : PacketContent
        {
            public long timestamp;
        }
    }

    public class PongPacket : Packet
    {
        public PongPacket()
        {
            Header = new PongHeader();
            Content = new PingPacket.PingContent { timestamp = DateTime.UtcNow.Ticks };
        }

        public class PongHeader : PacketHeader
        {
            public override string GetPacketType() => "pong";
        }
    }
}
