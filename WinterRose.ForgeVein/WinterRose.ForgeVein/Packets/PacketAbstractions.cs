using WinterRose.ForgeVein.Networking.Session;

namespace WinterRose.ForgeVein.Networking.Packets;

public enum PacketCategory
{
    Handshake,
    Control,
    Reliable,
    HotPath,
    Relay,
    Stream,
    Diagnostics,
    Authentication
}

public sealed record PacketMetadata(
    string Name,
    PacketCategory Category,
    ReliabilityMode Reliability,
    bool RequiresValidation,
    bool RequiresRouting);

public interface IPacketSerializer
{
    ReadOnlyMemory<byte> Serialize(object packet, PacketMetadata metadata);
    object? Deserialize(ReadOnlyMemory<byte> payload, PacketMetadata metadata);
}

public interface IPacketDescriptor
{
    Guid PacketId { get; }
    Type PacketType { get; }
    PacketMetadata Metadata { get; }
    IPacketSerializer Serializer { get; }

    object? Deserialize(ReadOnlyMemory<byte> packetPayload)
        => Serializer.Deserialize(packetPayload, Metadata);
    ReadOnlyMemory<byte> Serialize(object packet, PacketMetadata metadata)
        => Serializer.Serialize(packet, metadata);
}

public interface IPacketRegistry
{
    IEnumerable<IPacketDescriptor> RegisteredPackets { get; }
    void Register(IPacketDescriptor descriptor);
    bool TryGetDescriptor(Guid packetId, out IPacketDescriptor descriptor);
    bool TryGetDescriptor(Type packetType, out IPacketDescriptor descriptor);
    bool TryGetDescriptor(string name, out IPacketDescriptor descriptor);
}

public interface IPacketDispatcher
{
    ValueTask DispatchAsync(ISessionConnection session, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken = default);
}

public interface IPacketHandler<in TPacket>
{
    ValueTask HandleAsync(ISessionConnection session, TPacket packet, CancellationToken cancellationToken = default);
}
