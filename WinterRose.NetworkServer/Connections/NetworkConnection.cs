using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinterRose.ConsoleExtentions;
using WinterRose.Expressions;
using WinterRose.NetworkServer.Packets;
using WinterRose.Recordium;
using WinterRose.WinterForgeSerializing.Compiling;
using WinterRose.WinterForgeSerializing.Workers;

namespace WinterRose.NetworkServer;

public abstract class NetworkConnection
{
    internal static Dictionary<string, Type> packetHandlers = [];

    internal virtual Dictionary<Guid, Response<Packet>> pendingResponses { get; }

    public abstract bool IsConnected { get; }

    static NetworkConnection()
    {
        // Expand the static initializer so duplicate keys or other registration problems
        // produce a clear, debuggable error message identifying the offending types/keys.
        var handlerTypes = TypeWorker
            .FindTypesWithBase<PacketHandler>()
            .Where(handler => handler != typeof(PacketHandler))
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            try
            {
                var instance = (PacketHandler?)ActivatorExtra.CreateInstance(handlerType);
                if (instance == null)
                    continue; // skip if we couldn't create it for some reason

                string key = instance.Type ?? throw new InvalidOperationException($"PacketHandler '{handlerType.FullName}' returned a null/empty Type.");

                if (packetHandlers.ContainsKey(key))
                {
                    var existingType = packetHandlers[key];
                    string message = $"Duplicate packet handler key detected: '{key}'. Existing handler type: '{existingType.FullName}'. New handler type: '{handlerType.FullName}'.";
                    Debug.WriteLine(message);
                    Console.Error.WriteLine(message);
                    throw new InvalidOperationException(message);
                }

                packetHandlers[key] = handlerType;
            }
            catch (Exception ex)
            {
                string message = $"Error registering packet handler type '{handlerType.FullName}': {ex.GetType().Name} - {ex.Message}";
                Debug.WriteLine(message);
                Console.Error.WriteLine(message);
                // Re-throw to fail fast so you can debug the initialization issue.
                throw;
            }
        }
    }

    public NetworkConnection(Log log)
    {
        logger = log;
        ByteToOpcodeDecompiler.WaitIndefinitelyForData = true;
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
    public bool IsServer { get; internal protected set; }

    public readonly Log logger;

    /// <summary>
    /// The identifier for this connection.
    /// </summary>
    public virtual Guid Identifier { get; internal set; }

    public virtual string Username { get; set; } = "UNSET";

    internal object ReadFromNetworkStream(NetworkStream stream)
    {
        using MemoryStream mem = new();
        var instr = ByteToOpcodeDecompiler.Parse(stream);
        return new WinterForgeVM().Execute(instr, true);
    }

    /// <summary>
    /// Sends the given packet to this connection
    /// </summary>
    /// <param name="packet"></param>
    public abstract void Send(Packet packet, bool overrideSenderName = true);

    public abstract bool Send(Packet packet, Guid destination, bool overrideSenderName = true);

    public abstract bool TunnelRequestReceived(TunnelRequestPacket packet, NetworkConnection sender);
    public abstract void TunnelRequestAccepted(Guid a, Guid b);

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
            sender.Send(new NoPacket());
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

            if (p is RelayPacket)
            {
                HandleRelayPacket(packet, self);
                return;
            }

            if(p is TunnelAcceptedPacket)
            {
                TunnelRequestPacket.TunnelReqContent c = p.Content as TunnelRequestPacket.TunnelReqContent;
                TunnelRequestAccepted(c.from, c.to);
                return;
            }

            if(p is TunnelRequestPacket trp)
            {
                HandleTunnelRequest(self, sender, replyPacket, trp);
                return;
            }

            PacketHandler? h = GetHandler(p);
            if (h is null)
            {
                self.logger.Critical("ReplyHandler - No handler for packet type found in the application: " + p.Header.GetPacketType());
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
            logger.Critical("No handler for packet type found in the application: " + packet.Header.GetPacketType());
            return;
        }

        handler.Handle(packet, this, sender);
    }

    private void HandleTunnelRequest(NetworkConnection self, NetworkConnection sender, ReplyPacket replyPacket, TunnelRequestPacket trp)
    {
        if (TunnelRequestReceived(trp, sender))
        {
            var ap = new TunnelAcceptedPacket();
            ((TunnelRequestPacket.TunnelReqContent)ap.Content).from = 
                ((TunnelRequestPacket.TunnelReqContent)trp.Content).from;
            ((TunnelRequestPacket.TunnelReqContent)ap.Content).to = 
                ((TunnelRequestPacket.TunnelReqContent)trp.Content).to;

            sender.Send(replyPacket.Reply(ap, self));
            TunnelRequestPacket.TunnelReqContent? content = trp.Content as TunnelRequestPacket.TunnelReqContent;
            TunnelRequestAccepted(content!.from, content.to);
        }
        else
        {
            sender.Send(replyPacket.Reply(new NoPacket(), self));
        }
    }

    /// <summary>
    /// A method only used by server. but here anyway because of other abstractions
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="self"></param>
    private void HandleRelayPacket(Packet packet, NetworkConnection self)
    {
        RelayPacket.RelayContent content = packet.Content as RelayPacket.RelayContent;
        logger.Info($"Relaying packet {content.relayedPacket.GetType().Name} " +
            $"from {content.sender} to {content.destination}");

        if(!self.Send(packet, content.destination))
        {
            Guid answerGuid = content.relayedPacket.Header.CorrelationId;
            var no = new NoPacket();
            no.Header.CorrelationId = answerGuid;
            self.Send(ReplyPacket.CreateReply(no, self));
        }
    }

    public virtual Packet SendAndWaitForResponse(Packet packet, Guid? destination = null, TimeSpan? timeout = null)
    {
        var response = new Response<Packet>();
        packet.Header.CorrelationId = Guid.CreateVersion7();
        pendingResponses[packet.Header.CorrelationId] = response;

        if(destination is not null)
            Send(ReplyPacket.CreateReply(packet, this), destination.Value);
        else
            Send(ReplyPacket.CreateReply(packet, this));

        if (!response.Wait(timeout))
            throw new TimeoutException(Username + 
                " - A response was not given within the timeout: " 
                + timeout?.ToString() ?? "infinite");

        return response.Result;
    }

    public abstract NetworkStream GetStream();

    public TimeSpan Ping(TimeSpan? timeout = null)
    {
        DateTime now = DateTime.UtcNow;
        PongPacket pong = (PongPacket)SendAndWaitForResponse(new PingPacket(), timeout: timeout);
        DateTime roundTrip = new DateTime(((PingPacket.PingContent)pong.Content).timestamp);
        return roundTrip - now;
    }
}
