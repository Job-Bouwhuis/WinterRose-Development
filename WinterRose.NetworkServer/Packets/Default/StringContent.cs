using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.NetworkServer.Packets
{
    public class StringContent : PacketContent
    {
        public StringContent(string content)
        {
            Content = content;
        }
        private StringContent() { } // for serialization

        [WFInclude]
        public string Content { get; private set; }
    }
}
