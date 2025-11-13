using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using WinterRose.NetworkServer.Packets;

namespace WinterRose.NetworkServer.Packets.Default.Handlers;
internal class RelayPacketHandler : PacketHandler
{
    public override string Type => "RELAYPACKET";

    public override void Handle(Packet packet, NetworkConnection self, NetworkConnection sender)
    {
        RelayPacket.RelayContent content = packet.Content as RelayPacket.RelayContent;
        logger.Info($"Relaying packet {content.relayedPacket.GetType().Name} " +
            $"from {content.sender} to {content.destination}");
        sender.Send(packet, content.destination);
    }
    public override void HandleResponsePacket(ReplyPacket replyPacket, Packet packet, NetworkConnection self, NetworkConnection sender)
    {
        RelayPacket.RelayContent content = packet.Content as RelayPacket.RelayContent;
        logger.Info($"Relaying packet {content.relayedPacket.GetType().Name} " +
            $"from {content.sender} to {content.destination}");
        sender.Send(packet, content.destination);
    }
}
