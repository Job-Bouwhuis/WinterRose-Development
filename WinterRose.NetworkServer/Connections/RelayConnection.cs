using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WinterRose.NetworkServer.Packets;
using WinterRose.NetworkServer.Packets;

namespace WinterRose.NetworkServer.Connections;

public class RelayConnection : NetworkConnection
{
    private readonly NetworkConnection sendPoint;

    public ConnectionSource Source { get; private set; } = ConnectionSource.Unknown;

    public override bool IsConnected => sendPoint.IsConnected;

    /// <summary>
    /// <see cref="Guid.Empty"/> when <see cref="Source"/> is not <see cref="ConnectionSource.ClientRelay"/>
    /// </summary>
    public Guid RelayIdentifier { get; set; } = Guid.Empty;

    public override Guid Identifier { get => RelayIdentifier; internal set => RelayIdentifier = value; }

    internal override Dictionary<Guid, Response<Packet>> pendingResponses
        => sendPoint.pendingResponses;

    string? username = null;
    public override string Username
    {
        get
        {
            if(username == null)
            {
                GetUsernamePacket? p = sendPoint.SendAndWaitForResponse(
                    new GetUsernamePacket(RelayIdentifier), timeout: TimeSpan.FromSeconds(5)) as GetUsernamePacket;
                if (p is null)
                    return "Unknown Username";
                StringContent? content = p.Content as StringContent;
                username = content!.Content;
            }
            return username;
        }
        set
        {
            Packet p = sendPoint.SendAndWaitForResponse(new SetUsernamePacket(value));
            if (p is OkPacket)
                username = value;
        }
    }

    /// <summary>
    /// Creates a new RelayConnection. that sends its data using the sendpoint
    /// </summary>
    /// <param name="SendPoint">The connection through which data is sent</param>
    public RelayConnection(NetworkConnection SendPoint) : base(SendPoint.logger) => sendPoint = SendPoint;

    internal void SetSource(ConnectionSource source) => Source = source;

    public override void Send(Packet packet, bool overridePacketName = true) => sendPoint.Send(packet, RelayIdentifier, overridePacketName);

    public override bool Send(Packet packet, Guid destination, bool overridePacketName = true) => sendPoint.Send(packet, destination, overridePacketName);

    public override bool Disconnect() => sendPoint.Disconnect();

    public override NetworkStream GetStream() => throw new NotImplementedException();
    public override bool TunnelRequestReceived(TunnelRequestPacket packet, NetworkConnection sender) => sendPoint.TunnelRequestReceived(packet, sender);
    public override void TunnelRequestAccepted(Guid a, Guid b) => sendPoint.TunnelRequestAccepted(a, b);
}
