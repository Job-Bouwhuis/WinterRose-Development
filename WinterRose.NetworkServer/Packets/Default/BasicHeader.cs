using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.NetworkServer.Packets
{
    class BasicHeader : PacketHeader
    {
        [IncludeWithSerialization]
        private string id;

        public BasicHeader(string id) => this.id = id;
        private BasicHeader() { } // for serialization
        public override string GetPacketType() => id;
    }
}
