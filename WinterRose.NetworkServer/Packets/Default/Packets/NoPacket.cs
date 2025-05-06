using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.NetworkServer.Packets
{
    public class NoPacket : Packet
    {
        public NoPacket()
        {
            Header = new BasicHeader("NO");
            Content = new BasicContent();
        }
    }
}
