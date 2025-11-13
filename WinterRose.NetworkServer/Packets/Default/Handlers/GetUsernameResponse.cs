using System;
using System.Linq;
using WinterRose.NetworkServer.Connections;
using WinterRose.NetworkServer.Packets;

namespace WinterRose.NetworkServer.Packets.Default.Handlers;

internal class GetUsernameResponse : PacketHandler
{
    public override string Type => "GETUSERNAME";

    public override void Handle(Packet packet, NetworkConnection self, NetworkConnection sender)
    {
        logger.Warning("Username requested but sender does not expect a reply!");
    }

    public override void HandleResponsePacket(ReplyPacket replyPacket, Packet packet, NetworkConnection self, NetworkConnection sender)
    {
        if (self is not ServerConnection server)
        {
            if (self is ClientConnection c)
                sender.Send(replyPacket.Reply(new GetUsernamePacket(c.Username), self));
            else
                logger.Error("Unknown self to handle GETUSERNAME");
            return;
        }

        if (packet.Content is not GuidContent guidContent)
        {
            logger.Error("Invalid content for GetUsernamePacket (expected GuidContent).");
            return;
        }

        var client = server.GetClients().FirstOrDefault(c => c.Identifier == guidContent.guid);
        if (client == null)
        {
            logger.Warning($"Client with identifier {guidContent} not found.");
            sender.Send(replyPacket.Reply(new NoPacket(), self));
            return;
        }

        var username = client.Username;
        var responsePacket = new GetUsernamePacket(username);

        sender.Send(replyPacket.Reply(responsePacket, self));
    }
}
