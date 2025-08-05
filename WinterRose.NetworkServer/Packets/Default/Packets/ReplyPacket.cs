using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.NetworkServer.Packets
{
    public class ReplyPacket : Packet
    {
        public ReplyPacket()
        {
            Header = new ReplyHeader();
            Content = new ReplyContent();
        }

        public static Packet CreateReply(Packet packet, NetworkConnection self)
        {
            return new ReplyPacket()
            {
                Header = new ReplyHeader()
                {
                    CorrelationId = packet.Header.CorrelationId
                },
                Content = new ReplyContent()
                {
                    OriginalPacket = packet,
                    SenderID = self.Identifier
                }
            };
        }

        public Packet Reply(Packet packet, NetworkConnection self)
        {
            return new ReplyPacket()
            {
                Header = new ReplyHeader()
                {
                    CorrelationId = Header.CorrelationId
                },
                Content = new ReplyContent()
                {
                    OriginalPacket = packet,
                    SenderID = self.Identifier
                }
            };
        }

        public class ReplyHeader : PacketHeader
        {
            public override string GetPacketType() => "reply";
        }

        public class ReplyContent : PacketContent
        {
            [WFInclude]
            public Guid SenderID { get; set; }
            [WFInclude]
            public Packet? OriginalPacket { get; internal set; }
        }
    }

}
