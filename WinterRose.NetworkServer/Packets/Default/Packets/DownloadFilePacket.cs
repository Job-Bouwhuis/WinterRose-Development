using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.NetworkServer.Packets
{
    public class DownloadFilePacket : Packet
    {
        private DownloadFilePacket() { }
        public DownloadFilePacket(string path)
        {
            Header = new BasicHeader("DOWNLOADFILE");
            Content = new StringContent(path);
        }
    }
}
