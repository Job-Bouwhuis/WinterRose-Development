using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.NetworkServer.Packets.Default.Packets;
internal class GetUsernamePacket : Packet
{
    private GetUsernamePacket() { } // for serialization

    /// <summary>
    /// Client side call
    /// </summary>
    /// <param name="identifier"></param>
    public GetUsernamePacket(Guid identifier)
    {
        Header = new BasicHeader("GETUSERNAME");
        Content = new GuidContent(identifier);
    }

    /// <summary>
    /// Server side call
    /// </summary>
    /// <param name="username"></param>
    public GetUsernamePacket(string username)
    {
        Header = new BasicHeader("GETUSERNAME");
        Content = new StringContent(username);
    }


}
