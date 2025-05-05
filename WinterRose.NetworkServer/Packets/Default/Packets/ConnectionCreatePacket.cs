using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.NetworkServer.Packets.Default.Packets
{
    internal class ConnectionCreatePacket : Packet
    {
        internal ConnectionCreatePacket(Guid guid)
        {
            Header = new PHeader();
            Content = new PContent()
            {
                guid = guid
            };
        }

        private ConnectionCreatePacket() { } // for serialization

        public class PHeader : PacketHeader
        {
            public override string GetPacketType() => "CONNECTIONCREATE";
        }

        public class PContent : PacketContent
        {
            public Guid guid;
        }
    }
}
