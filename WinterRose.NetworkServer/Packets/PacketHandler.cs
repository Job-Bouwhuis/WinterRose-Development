using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Recordium;

namespace WinterRose.NetworkServer.Packets
{
    public abstract class PacketHandler
    {
        public abstract string Type { get; }

        internal protected Log logger = null!;

        public abstract void Handle(Packet packet, NetworkConnection self, NetworkConnection sender);

        // New abstract method to handle response packets
        public abstract void HandleResponsePacket(ReplyPacket replyPacket, Packet packet, NetworkConnection self, NetworkConnection sender);

        protected PacketHandler? GetHandler(Packet packet)
        {
            if (NetworkConnection.packetHandlers.TryGetValue(packet.Header.GetPacketType(), out Type? handlerType))
            {
                PacketHandler? handler = (PacketHandler?)ActivatorExtra.CreateInstance(handlerType);
                if (handler == null)
                    return handler;
                handler.logger = logger;
                return handler;
            }
            return null;
        }
    }
}
