using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.NetworkServer.Packets
{
    internal class ConnectionCreatePacket : Packet
    {
        internal ConnectionCreatePacket(Guid guid)
        {
            Header = new BasicHeader("CONNECTIONCREATE");
            Content = new PContent(guid);
        }

        private ConnectionCreatePacket() { } // for serialization

        public class PContent : PacketContent
        {
            public Guid guid;

            public PContent(Guid guid)
            {
                this.guid = guid;
            }
        }
    }
}
