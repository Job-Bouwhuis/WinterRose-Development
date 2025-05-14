using System.Collections.Generic;
using System.Linq;

namespace WinterRose.NetworkServer.Connections;
public class TunnelConnectionHandler
{
    List<TunnelPair> activeTunnels = [];

    Dictionary<NetworkConnection, TunnelPair> lookup = [];

    public void DefineTunnel(NetworkConnection a, NetworkConnection b)
    {
        var pair = new TunnelPair(a, b, this);
        activeTunnels.Add(pair);
        _ = pair.StartTunnelAsync();
        lookup.Add(a, pair);
        lookup.Add(b, pair);
    }

    public void CloseTunnel(NetworkConnection AorB)
    {
        var pair = lookup[AorB];
        activeTunnels.Remove(pair);
        lookup.Remove(AorB);
        if (AorB == pair.A)
            lookup.Remove(pair.B);
        else
            lookup.Remove(pair.A);
    }

    public bool InTunnel(NetworkConnection connection) => lookup.TryGetValue(connection, out _);
}