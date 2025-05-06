using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WinterRose.NetworkServer.Packets;

namespace WinterRose.NetworkServer.Connections
{
    class ReplyConnection : NetworkConnection
    {
        private readonly NetworkConnection original;

        public ReplyConnection(NetworkConnection original) : base(original.logger)
        {
            this.original = original;
        }

        public override bool Disconnect() => original.Disconnect();
        public override NetworkStream GetStream() => original.GetStream();

        public override void Send(Packet packet)
        {
            if (packet is not ReplyPacket p)
                throw new ArgumentException("Sent packet is not a ReplyPacket!");

            original.Send(packet);
        }

        public override bool Send(Packet packet, Guid destination)
        {
            return original.Send(packet, destination);
        }
    }
}
