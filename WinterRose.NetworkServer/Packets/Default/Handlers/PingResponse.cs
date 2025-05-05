using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.NetworkServer.Packets.Default.Packets;

namespace WinterRose.NetworkServer.Packets
{
    internal class PingResponse : PacketHandler
    {
        public override string Type => "ping";

        public override void Handle(Packet packet, NetworkConnection self, NetworkConnection sender)
        {
            Console.WriteLine($"Ping from client '{sender.Identifier}' - {new DateTime(((PingPacket.PingContent)packet.Content).timestamp)}ms");
            sender.Send(new PongPacket());
        }

        public override void HandleResponsePacket(ReplyPacket replyPacket, Packet packet, NetworkConnection self, NetworkConnection sender)
        {
            sender.Send(replyPacket.Reply(new PongPacket(), self));
        }
    }

    internal class PongResponse : PacketHandler
    {
        public override string Type => "pong";

        public override void Handle(Packet packet, NetworkConnection self, NetworkConnection sender)
        {
            Console.WriteLine($"Pong '{sender.Identifier}' - {new DateTime(((PingPacket.PingContent)packet.Content).timestamp)}ms");
        }

        public override void HandleResponsePacket(ReplyPacket replyPacket, Packet packet, NetworkConnection self, NetworkConnection sender)
        {
            sender.Send(new OkPacket());
        }
    }
}
