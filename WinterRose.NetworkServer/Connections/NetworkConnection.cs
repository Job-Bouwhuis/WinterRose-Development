using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinterRose.ConsoleExtentions;
using WinterRose.NetworkServer.Packets;

namespace WinterRose.NetworkServer;

public abstract class NetworkConnection
{
    private static Dictionary<string, Type> packetHandlers = [];

    private readonly Dictionary<Guid, Response<Packet>> pendingResponses = new();

    static NetworkConnection() =>
        TypeWorker.FindTypesWithBase<PacketHandler>().Where(handler => handler != typeof(PacketHandler))
            .Foreach(handler => packetHandlers.Add(
                ((PacketHandler)ActivatorExtra.CreateInstance(handler)).Type, handler));

    /// <summary>
    /// Whether or not this connection is a server connection
    /// </summary>
    public bool IsServer { get; protected set; }

    /// <summary>
    /// The identifier for this connection.
    /// </summary>
    public Guid Identifier { get; protected set; }

    public string Username { get; set; } = "UNSET";

    /// <summary>
    /// Sends the given packet to this connection
    /// </summary>
    /// <param name="packet"></param>
    public abstract void Send(Packet packet);

    public abstract void Send(Packet packet, Guid destination);

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
    internal static PacketHandler? GetHandler(Packet packet)
    {
        if (packetHandlers.TryGetValue(packet.Header.GetPacketType(), out Type? handlerType))
            return (PacketHandler?)ActivatorExtra.CreateInstance(handlerType);
        return null;
    }

    protected void HandleReceivedPacket(Packet packet, NetworkConnection self, NetworkConnection sender)
    {
        if (packet.Header.GetPacketType() == "")
        {
            sender.Send(new OkPacket());
            return;
        }
        PacketHandler? handler = GetHandler(packet);
        if (handler is null)
        {
            ConsoleS.WriteErrorLine("No handler for packet type found in the application: " + packet.Header.GetPacketType());
            return;
        }

        if (pendingResponses.TryGetValue(packet.Header.CorrelationId, out var response))
        {
            ReplyPacket.ReplyContent content = packet.Content as ReplyPacket.ReplyContent;
            response.SetResult(content.OriginalPacket);
            return; // packet was a response. skip calling handlers
        }

        handler.Handle(packet, this, sender);
    }

    public Packet SendAndWaitForResponse(Packet packet, TimeSpan? timeout = null)
    {
        var response = new Response<Packet>();
        packet.Header.CorrelationId = Guid.NewGuid(); // Unique ID for this request/response pair.

        // Store the response object for the corresponding request.
        pendingResponses[packet.Header.CorrelationId] = response;

        Send(ReplyPacket.CreateReply(packet, this));

        bool timeoutPassed = false;

        Task waiter = Task.Run(() =>
        {
            while (!response.IsCompleted) ;
        });

        waiter.Wait(((int?)timeout?.TotalMilliseconds) ?? Timeout.Infinite, new CancellationToken());

        if (timeoutPassed)
            throw new TimeoutException("A response was not given within the timeout: " + timeout.ToString());

        return response.Result;
    }

}
