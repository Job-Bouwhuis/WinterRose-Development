using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ConsoleExtentions;
using WinterRose.Networking.TCP;

namespace WinterRose.NetworkServer.Packets.Default.Responses
{
    public class ReplyPacketHandler : PacketHandler
    {
        public override string Type => "reply";

        public override void Handle(Packet packet, NetworkConnection self, NetworkConnection sender)
        {
            HandleResponsePacket((ReplyPacket)packet, null, self, sender);
        }

        // Override this method to process reply packets
        public override void HandleResponsePacket(ReplyPacket replyPacket, Packet? p, NetworkConnection self, NetworkConnection sender)
        {
            // Extract the original packet (this could be a PingPacket, etc.)
            Packet? packet = ((ReplyPacket.ReplyContent)replyPacket.Content).OriginalPacket 
                ?? throw new NotImplementedException();

            PacketHandler? handler = NetworkConnection.GetHandler(packet);
            if (handler is null)
            {
                ConsoleS.WriteErrorLine("No handler for packet type found in the application: " + packet.Header.GetPacketType());
                return;
            }

            handler.HandleResponsePacket(replyPacket, packet, self, sender);
        }
    }
}
