using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.NetworkServer.Connections;
using WinterRose.NetworkServer.Packets;

namespace WinterRose.NetworkServer.Packets.Default.Handlers;
internal class ConnectionListHandler : PacketHandler
{
    public override string Type => "CONNECTIONLIST";

    public override void Handle(Packet packet, NetworkConnection self, NetworkConnection sender)
    {
        if (self is not ServerConnection server)
        {
            logger.Warning($"Client '{sender.Identifier}' tried to get connection list (reply), but only server may handle this.");
            return;
        }
        logger.Warning($"ConnectionList request sent without the sender expecting a reply.");
    }

    public override void HandleResponsePacket(ReplyPacket replyPacket, Packet packet, NetworkConnection self, NetworkConnection sender)
    {
        if (self is not ServerConnection server)
        {
            logger.Warning($"Client '{sender.Identifier}' tried to get connection list (reply), but only server may handle this.");
            return;
        }

        var clientIds = server.GetClients().Select(c => c.Identifier).ToList();
        var response = new ConnectionListPacket(clientIds);
        sender.Send(replyPacket.Reply(response, self));
    }
}
