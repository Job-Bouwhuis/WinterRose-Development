using System.Text;
using WinterRose.ForgeVein.Networking.Packets;
using WinterRose.WinterForgeSerializing;

namespace WinterRose.ForgeVein.Networking.Defaults;

public sealed class WinterForgePacketSerializer : IPacketSerializer
{
    private static ObjectPool<MemoryStream> memoryStreamPool = new(resetAction: stream => stream.SetLength(0));

    public ReadOnlyMemory<byte> Serialize(object packet, PacketMetadata metadata)
    {
        if (packet is null)
            return ReadOnlyMemory<byte>.Empty;

        using var buffer = memoryStreamPool.Using();
        WinterForge.SerializeToStream(packet, buffer, TargetFormat.Optimized);
        
        return buffer.Item.ToArray();
    }

    public object? Deserialize(ReadOnlyMemory<byte> payload, PacketMetadata metadata)
    {
        if (payload.IsEmpty)
            return default;

        using var buffer = memoryStreamPool.Using();
        buffer.Item.Write(payload.Span);
        buffer.Item.Position = 0;
        var res = WinterForge.DeserializeFromStream(buffer.Item);
        if (res is Nothing)
            return null;
        else 
            return res;
    }
}
