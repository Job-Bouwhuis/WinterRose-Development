using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.NetworkServer.Packets
{
    public class TunnelRequestPacket : Packet
    {
        private TunnelRequestPacket() { } // for serialization
        public TunnelRequestPacket(Guid from, Guid to, string? reason = null)
        {
            Header = new BasicHeader("TUNNELREQUEST");
            Content = new TunnelReqContent()
            {
                from = from,
                to = to,
                reason = reason
            };
        }

        public class TunnelReqContent : PacketContent
        {
            public Guid from;
            public Guid to;
            public string? reason;
        }
    }
}
