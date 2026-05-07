using WinterRose.ForgeVein.Networking.Packets;
using WinterRose.ForgeVein.Networking.Session;
using WinterRose.Recordium;

namespace WinterRose.ForgeVein.Networking.Defaults;

public sealed class DefaultPacketDispatcher : IPacketDispatcher
{
    private readonly IPacketRegistry registry;
    private readonly IPacketHandlerRegistry handlerRegistry;
    private readonly Log logger;

    public DefaultPacketDispatcher(IPacketRegistry registry, IPacketHandlerRegistry handlerRegistry, Log logger)
    {
        this.registry = registry;
        this.handlerRegistry = handlerRegistry;
        this.logger = logger;
    }

    public ValueTask DispatchAsync(ISessionConnection session, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken = default)
    {
        if (!TryReadPacketId(payload, out var packetId, out var packetPayload))
        {
            logger.Warning("Packet dispatch failed: missing packet id.");
            return ValueTask.CompletedTask;
        }

        if (!registry.TryGetDescriptor(packetId, out var descriptor))
        {
            logger.Warning($"Packet dispatch failed: unknown packet id {packetId}.");
            return ValueTask.CompletedTask;
        }

        object? packet = descriptor.Deserialize(packetPayload);
        DispatchToHandler(session, descriptor, packet, cancellationToken);
        return ValueTask.CompletedTask;
    }

    private static bool TryReadPacketId(ReadOnlyMemory<byte> payload, out Guid packetId, out ReadOnlyMemory<byte> packetPayload)
    {
        if (payload.Length < 16)
        {
            packetId = Guid.Empty;
            packetPayload = ReadOnlyMemory<byte>.Empty;
            return false;
        }

        packetId = new Guid(payload[..16].Span);
        packetPayload = payload[16..];
        return true;
    }

    private void DispatchToHandler(ISessionConnection session, IPacketDescriptor descriptor, object packet, CancellationToken cancellationToken)
    {
        var handler = handlerRegistry.GetType().GetMethod("TryGetHandler")!
            .MakeGenericMethod(descriptor.PacketType)
            .Invoke(handlerRegistry, Array.Empty<object?>());

        if (handler is bool { } success && success)
        {
            var handlerProperty = handlerRegistry.GetType().GetMethod("TryGetHandler")!
                .MakeGenericMethod(descriptor.PacketType)
                .Invoke(handlerRegistry, Array.Empty<object?>());
        }

        var tryGet = handlerRegistry.GetType().GetMethod("TryGetHandler")!
            .MakeGenericMethod(descriptor.PacketType);

        var parameters = new object?[] { null };
        var found = (bool)tryGet.Invoke(handlerRegistry, parameters)!;
        if (!found || parameters[0] is null)
        {
            logger.Warning($"No handler registered for packet {descriptor.Metadata.Name}.");
            return;
        }

        var handlerInstance = parameters[0];
        var handleMethod = handlerInstance.GetType().GetMethod("HandleAsync")!;
        handleMethod.Invoke(handlerInstance, new object[] { session, packet, cancellationToken });
    }
}
