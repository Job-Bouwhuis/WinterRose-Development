using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WinterRose.NetworkServer.Packets;

namespace WinterRose.NetworkServer.Connections;

public class RelayConnection : NetworkConnection
{
    private readonly NetworkConnection sendPoint;

    public ConnectionSource Source { get; private set; } = ConnectionSource.Unknown;

    /// <summary>
    /// not null when <see cref="Source"/> is <see cref="ConnectionSource.ClientRelay"/>
    /// </summary>
    public string? RelayIdentifier { get; set; } = null;

    /// <summary>
    /// Creates a new RelayConnection. that sends its data using the sendpoint
    /// </summary>
    /// <param name="SendPoint">The connection through which data is sent</param>
    public RelayConnection(NetworkConnection SendPoint)
    {
        sendPoint = SendPoint;
    }

    internal void SetSource(ConnectionSource source) => Source = source;

    public override void Send(Packet packet)
    {
        // TODO: make make RelayPacket, and make server react to this packet
        sendPoint.Send(packet);
    }

    public override void Send(Packet packet, Guid destination)
    {
        sendPoint.Send(packet, destination);
    }
}
