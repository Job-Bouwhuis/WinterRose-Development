using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinterRose.ConsoleExtentions;
using WinterRose.NetworkServer.Packets;
using WinterRose.NetworkServer.Packets.Default.Packets;

namespace WinterRose.NetworkServer;

public abstract class NetworkConnection
{
    internal static Dictionary<string, Type> packetHandlers = [];

    internal virtual Dictionary<Guid, Response<Packet>> pendingResponses { get; }

    static NetworkConnection() =>
        TypeWorker.FindTypesWithBase<PacketHandler>().Where(handler => handler != typeof(PacketHandler))
            .Foreach(handler => packetHandlers.Add(
                ((PacketHandler)ActivatorExtra.CreateInstance(handler)).Type, handler));

    public NetworkConnection(ILogger logger)
    {
        this.logger = logger;
        pendingResponses = [];

        Windows.ApplicationExit += () =>
        {
            try
            {
                Disconnect();
            }
            catch // ignore any exception
            {

            }
        };
    }

    /// <summary>
    /// Whether or not this connection is a server connection
    /// </summary>
    public bool IsServer { get; protected set; }

    public readonly ILogger logger;

    /// <summary>
    /// The identifier for this connection.
    /// </summary>
    public virtual Guid Identifier { get; internal set; }

    public virtual string Username { get; set; } = "UNSET";

    /// <summary>
    /// Sends the given packet to this connection
    /// </summary>
    /// <param name="packet"></param>
    public abstract void Send(Packet packet);

    public abstract bool Send(Packet packet, Guid destination);

    /// <summary>
    /// When overriden in a derived class. attempts to disconnect the connection from the remote host
    /// </summary>
    /// <returns>true when accepted by the remote host, false otherwise. False may indicate crutial processing is still happening</returns>
    public abstract bool Disconnect();

    /// <summary>
    /// Gets a handler for the specific packet, if it exists
    /// </summary>
    /// <param name="packet"></param>
    /// <returns></returns>
    internal PacketHandler? GetHandler(Packet packet)
    {
        if (packetHandlers.TryGetValue(packet.Header.GetPacketType(), out Type? handlerType))
        {
            PacketHandler? handler = (PacketHandler?)ActivatorExtra.CreateInstance(handlerType);
            if (handler == null)
                return handler;
            handler.logger = logger;
            return handler;
        }
        return null;
    }

    public virtual void HandleReceivedPacket(Packet packet, NetworkConnection self, NetworkConnection sender)
    {
        if (packet.Header.GetPacketType() == "")
        {
            sender.Send(new OkPacket());
            return;
        }

        // if correlation ID does not exist. its a request, rather than an answer
        if (self.pendingResponses.TryGetValue(packet.Header.CorrelationId, out var response))
        {
            ReplyPacket.ReplyContent content = packet.Content as ReplyPacket.ReplyContent;
            response.SetResult(content.OriginalPacket);
            self.pendingResponses.Remove(packet.Header.CorrelationId);
            return; // packet was a response. skip calling handlers
        }

        if(packet is ReplyPacket replyPacket)
        {
            Packet? p = ((ReplyPacket.ReplyContent)replyPacket.Content).OriginalPacket
                ?? throw new NotImplementedException();

            PacketHandler? h = GetHandler(p);
            if (h is null)
            {
                self.logger.LogCritical("ReplyHandler - No handler for packet type found in the application: " + p.Header.GetPacketType());
                return;
            }

            if (p is RelayPacket)
            {
                HandleRelayPacket(packet, self);
                return;
            }

            h.HandleResponsePacket(replyPacket, p, self, sender);
            return;
        }

        if (packet is RelayPacket)
        {
            HandleRelayPacket(packet, self);
            return;
        }

        PacketHandler? handler = GetHandler(packet);
        if (handler is null)
        {
            logger.LogCritical("No handler for packet type found in the application: " + packet.Header.GetPacketType());
            return;
        }

        handler.Handle(packet, this, sender);
    }

    /// <summary>
    /// A method only used by server. but here anyway because of other abstractions
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="self"></param>
    private void HandleRelayPacket(Packet packet, NetworkConnection self)
    {
        RelayPacket.RelayContent content = packet.Content as RelayPacket.RelayContent;
        logger.LogInformation($"Relaying packet {content.relayedPacket.GetType().Name} " +
            $"from {content.sender} to {content.destination}");

        if(!self.Send(packet, content.destination))
        {
            Guid answerGuid = content.relayedPacket.Header.CorrelationId;
            var no = new NoPacket();
            no.Header.CorrelationId = answerGuid;
            self.Send(ReplyPacket.CreateReply(no, self));
        }
    }

    public virtual Packet SendAndWaitForResponse(Packet packet, TimeSpan? timeout = null)
    {
        var response = new Response<Packet>();
        packet.Header.CorrelationId = Guid.NewGuid(); // Unique ID for this request/response pair.
        // Store the response object for the corresponding request.
        pendingResponses[packet.Header.CorrelationId] = response;

        Send(ReplyPacket.CreateReply(packet, this));

        if (!response.Wait(timeout))
            throw new TimeoutException(Username + 
                " - A response was not given within the timeout: " 
                + timeout?.ToString() ?? "infinite");

        return response.Result;
    }

    public abstract NetworkStream GetStream();

    public TimeSpan Ping()
    {
        DateTime now = DateTime.UtcNow;
        PongPacket pong = 
            (PongPacket)SendAndWaitForResponse(new PingPacket()
            //, TimeSpan.FromSeconds(2)
            );
        DateTime roundTrip = new DateTime(((PingPacket.PingContent)pong.Content).timestamp);
        return roundTrip - now;
    }
}
