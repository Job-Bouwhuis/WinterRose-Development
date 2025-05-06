using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WinterRose.NetworkServer.Packets;
using WinterRose.NetworkServer.Packets.Default.Packets;

namespace WinterRose.NetworkServer.Connections;

public class RelayConnection : NetworkConnection
{
    private readonly NetworkConnection sendPoint;

    public ConnectionSource Source { get; private set; } = ConnectionSource.Unknown;

    /// <summary>
    /// not null when <see cref="Source"/> is <see cref="ConnectionSource.ClientRelay"/>
    /// </summary>
    public Guid? RelayIdentifier { get; set; } = null;

    public override Guid Identifier { get => sendPoint.Identifier; internal set => sendPoint.Identifier = value; }

    internal override Dictionary<Guid, Response<Packet>> pendingResponses
        => sendPoint.pendingResponses;

    string? username = null;
    public override string Username
    {
        get
        {
            if(username == null)
            {
                GetUsernamePacket p = sendPoint.SendAndWaitForResponse(
                    new GetUsernamePacket(RelayIdentifier.Value)) as GetUsernamePacket;
                StringContent content = p.Content as StringContent;
                username = content.Content;
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
    public RelayConnection(NetworkConnection SendPoint) : base(SendPoint.logger)
    {
        sendPoint = SendPoint;
        
    }

    internal void SetSource(ConnectionSource source) => Source = source;

    public override void Send(Packet packet)
    {
        sendPoint.Send(packet, RelayIdentifier!.Value);
    }

    public override bool Send(Packet packet, Guid destination)
    {
        return sendPoint.Send(packet, destination);
    }

    public override bool Disconnect()
    {
        return sendPoint.Disconnect();
    }

    //public override void HandleReceivedPacket(Packet packet, NetworkConnection self, NetworkConnection sender)
    //{
    //    sendPoint.HandleReceivedPacket(packet, self, sender);
    //}

    //public override Packet SendAndWaitForResponse(Packet packet, TimeSpan? timeout = null)
    //{
    //    if (packet is RelayPacket relay)
    //    {
    //        if (relay.Content is RelayPacket.RelayContent relayContent)
    //        {
    //            relayContent.sender = Identifier;
    //            relayContent.destination = RelayIdentifier.Value;
    //        }
    //    }
    //    else
    //        packet = new RelayPacket(packet, Identifier, RelayIdentifier.Value);

    //    return sendPoint.SendAndWaitForResponse(packet, timeout);
    //}

    public override NetworkStream GetStream() => throw new NotImplementedException();
}
