using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.NetworkServer.Packets;

namespace WinterRose.NetworkServer.Tests;
internal class MessageHandler : PacketHandler
{
    public override string Type => "Message";

    public override void Handle(Packet packet, NetworkConnection self, NetworkConnection sender)
    {
        Console.WriteLine(packet.SenderUsername + ": "+ ((Packets.StringContent)packet.Content).Content);
    }
    public override void HandleResponsePacket(ReplyPacket replyPacket, Packet packet, NetworkConnection self, NetworkConnection sender)
    {
        Console.WriteLine(replyPacket.SenderUsername + ": " + ((Packets.StringContent)packet.Content).Content);
        sender.Send(replyPacket.Reply(new OkPacket(), self));
    }
}
