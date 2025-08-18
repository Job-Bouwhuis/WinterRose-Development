using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeVein.Packets;
public abstract class Packet
{
    [WFInclude]
    public Guid PacketId { get; private set; } = Guid.NewGuid();

    [WFInclude]
    public Guid? ReplyToPacketId { get; set; } = null; // For reply packets

    [WFInclude]
    public abstract string PacketType { get; } // Unique string ID for packet type

    protected Packet() { }

    protected Packet(Guid? replyToPacketId)
    {
        ReplyToPacketId = replyToPacketId;
    }
}

