using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Serialization;

namespace WinterRose.Networking.TCP
{
    public class Packet
    {
        [IncludeWithSerialization]
        public TCPClientInfo? Sender { get; internal set; }

        [IncludeWithSerialization]
        public bool FromServer { get; private set; }

        [IncludeWithSerialization]
        public string? Payload {get; private set; }

        public Packet(string? payload, TCPClientInfo senderInfo)
        {
            Payload = payload;
            Sender = senderInfo;
            FromServer = false;
        }

        public Packet(string? payload) : this(payload, null)
        {
            FromServer = true;
        }

        public Packet(string? payload, TCPClientInfo senderInfo, bool fromServer) : this(payload, senderInfo)
        {
            FromServer = fromServer;
        }

        public Packet() { }

        public static Packet FromSerialized(string serialziedPacket)
        {
            return SnowSerializer.Deserialize<Packet>(serialziedPacket).Result;
        }

        public SerializationResult GetSerialized()
        {
            return SnowSerializer.Serialize(this);
        }
    }
}
