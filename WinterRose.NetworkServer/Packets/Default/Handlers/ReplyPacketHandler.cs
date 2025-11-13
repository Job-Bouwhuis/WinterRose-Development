using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ConsoleExtentions;

namespace WinterRose.NetworkServer.Packets.Default.Responses
{
    public class ReplyPacketHandler : PacketHandler
    {
        public override string Type => "-reply";

        public override void Handle(Packet packet, NetworkConnection self, NetworkConnection sender)
        {
            HandleResponsePacket((ReplyPacket)packet, null, self, sender);
        }

        public override void HandleResponsePacket(ReplyPacket replyPacket, Packet? p, NetworkConnection self, NetworkConnection sender)
        {
            Packet? packet = ((ReplyPacket.ReplyContent)replyPacket.Content).OriginalPacket 
                ?? throw new NotImplementedException();

            PacketHandler? handler = GetHandler(packet);
            if (handler is null)
            {
                logger.Critical("ReplyHandler - No handler for packet type found in the application: " + packet.Header.GetPacketType());
                return;
            }

            handler.HandleResponsePacket(replyPacket, packet, self, sender);
        }
    }
}
