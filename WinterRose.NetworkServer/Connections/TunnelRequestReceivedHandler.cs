using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.NetworkServer.Packets;

namespace WinterRose.NetworkServer.Connections
{
    public class TunnelRequestReceivedHandler
    {
        private List<Func<TunnelRequestPacket.TunnelReqContent, bool>> funcs = [];

        public void Add(Func<TunnelRequestPacket.TunnelReqContent, bool> handler)
        {
            funcs.Add(handler);
        }

        internal bool Invoke(TunnelRequestPacket.TunnelReqContent content)
        {
            foreach (var f in funcs)
                if (!f(content))
                    return false;
            return true;
        }
    }
}
