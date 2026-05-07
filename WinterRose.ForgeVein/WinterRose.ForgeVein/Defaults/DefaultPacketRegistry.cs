using System.Collections.Concurrent;
using WinterRose.ForgeVein.Networking.Packets;

namespace WinterRose.ForgeVein.Networking.Defaults;

public sealed class DefaultPacketDescriptor : IPacketDescriptor
{
    public DefaultPacketDescriptor(Guid packetId, Type packetType, PacketMetadata metadata, IPacketSerializer serializer)
    {
        PacketId = packetId;
        PacketType = packetType;
        Metadata = metadata;
        Serializer = serializer;
    }

    public Guid PacketId { get; }
    public Type PacketType { get; }
    public PacketMetadata Metadata { get; }
    public IPacketSerializer Serializer { get; }
}

public sealed class DefaultPacketRegistry : IPacketRegistry
{
    private readonly ConcurrentDictionary<Guid, IPacketDescriptor> byId = new();
    private readonly ConcurrentDictionary<Type, IPacketDescriptor> byType = new();
    private readonly ConcurrentDictionary<string, IPacketDescriptor> byName = new(StringComparer.OrdinalIgnoreCase);

    public IEnumerable<IPacketDescriptor> RegisteredPackets => byId.Values;

    public void Register(IPacketDescriptor descriptor)
    {
        if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
        if (!byId.TryAdd(descriptor.PacketId, descriptor))
            throw new InvalidOperationException($"Packet id already registered: {descriptor.PacketId}");
        if (!byType.TryAdd(descriptor.PacketType, descriptor))
            throw new InvalidOperationException($"Packet type already registered: {descriptor.PacketType}");
        if (!byName.TryAdd(descriptor.Metadata.Name, descriptor))
            throw new InvalidOperationException($"Packet name already registered: {descriptor.Metadata.Name}");
    }

    public bool TryGetDescriptor(Guid packetId, out IPacketDescriptor descriptor) => byId.TryGetValue(packetId, out descriptor!);
    public bool TryGetDescriptor(Type packetType, out IPacketDescriptor descriptor) => byType.TryGetValue(packetType, out descriptor!);
    public bool TryGetDescriptor(string name, out IPacketDescriptor descriptor) => byName.TryGetValue(name, out descriptor!);
}
