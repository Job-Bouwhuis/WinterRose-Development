using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.NetworkServer.Packets.Default.Handlers
{
    class DownloadFileHandler : PacketHandler
    {
        public override string Type => "DOWNLOADFILE";

        public override void Handle(Packet packet, NetworkConnection self, NetworkConnection sender)
        {
            string fileName = ((StringContent)packet.Content).Content;
            FilePacket filepacket = new(new FileInfo(fileName));
            sender.Send(filepacket);
        }

        public override void HandleResponsePacket(ReplyPacket replyPacket, Packet packet, NetworkConnection self, NetworkConnection sender)
        {
            string fileName = ((StringContent)packet.Content).Content.Replace(',', '.');
            FilePacket filepacket = new(new FileInfo(fileName));
            sender.Send(replyPacket.Reply(filepacket, self));
        }
    }
}
