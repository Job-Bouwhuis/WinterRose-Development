using System.Collections.Concurrent;
using WinterRose.ForgeVein.Networking.Packets;

namespace WinterRose.ForgeVein.Networking.Application;

public sealed class DefaultPacketHandlerRegistry : IPacketHandlerRegistry
{
    private readonly ConcurrentDictionary<Type, object> handlers = new();

    public void Register<TPacket>(IPacketHandler<TPacket> handler)
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        if (!handlers.TryAdd(typeof(TPacket), handler))
            throw new InvalidOperationException($"Handler already registered for packet type {typeof(TPacket).Name}.");
    }

    public bool TryGetHandler<TPacket>(out IPacketHandler<TPacket>? handler)
    {
        handler = null;
        if (handlers.TryGetValue(typeof(TPacket), out var found) && found is IPacketHandler<TPacket> typed)
        {
            handler = typed;
            return true;
        }

        return false;
    }
}
