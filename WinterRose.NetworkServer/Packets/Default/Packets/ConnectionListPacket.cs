using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.NetworkServer.Packets;
public class ConnectionListPacket : Packet
{
    /// <summary>
    /// Use this constructor when on the client asking the connections from the server
    /// </summary>
    public ConnectionListPacket()
    {
        Header = new ConnectionListHeader();
        Content = new ConnectionListContent();
    }

    /// <summary>
    /// Use this constructor to reply to a client request
    /// </summary>
    /// <param name="connections"></param>
    public ConnectionListPacket(List<Guid> connections)
    {
        Header = new ConnectionListHeader();
        Content = new ConnectionListContent { connections = connections };
    }

    public class ConnectionListHeader : PacketHeader
    {
        public override string GetPacketType() => "CONNECTIONLIST";
    }

    public class ConnectionListContent : PacketContent
    {
        public List<Guid> connections;
    }
}

