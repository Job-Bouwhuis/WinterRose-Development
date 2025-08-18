using System.Net.Sockets;
using WinterRose.ForgeVein.Packets;
using WinterRose.WinterForgeSerializing;

namespace WinterRose.ForgeVein.Connections;

public abstract class Connection : IDisposable
{
    public abstract NetworkStream Stream { get; protected set; }
    public abstract bool IsConnected { get; }

    private readonly MemoryStream receiveBuffer = new();

    public event Action<Packet> PacketReceived;

    public void StartListening()
    {
        Task.Run(async () =>
        {
            try
            {
                while (IsConnected)
                {
                    var packet = await ReadPacketAsync();
                    PacketReceived?.Invoke(packet);
                }
            }
            catch { Disconnect(); }
        });
    }

    private async Task<Packet?> ReadPacketAsync(NetworkStream networkStream, CancellationToken cancellationToken)
    {
        var tempBuffer = new byte[8192];

        while (true)
        {
            // Read some bytes from network, non-blocking except for data availability
            int bytesRead = await networkStream.ReadAsync(tempBuffer, 0, tempBuffer.Length, cancellationToken);
            if (bytesRead == 0)
                return null; // connection closed

            // Append to internal buffer
            long oldLength = receiveBuffer.Length;
            receiveBuffer.Position = oldLength;
            await receiveBuffer.WriteAsync(tempBuffer, 0, bytesRead, cancellationToken);
            receiveBuffer.Position = 0;

            // Try deserializing from buffer's start
            try
            {
                var packet = WinterForge.DeserializeFromStream<Packet>(receiveBuffer);
                if (packet != null)
                {
                    // Deserialization consumed some bytes, so we need to remove those from the buffer

                    // Get the position after deserialization
                    long afterPos = receiveBuffer.Position;

                    // Copy remaining bytes into new buffer
                    var remaining = new MemoryStream();
                    if (receiveBuffer.Length > afterPos)
                    {
                        receiveBuffer.Position = afterPos;
                        await receiveBuffer.CopyToAsync(remaining, cancellationToken);
                    }

                    // Reset receiveBuffer and copy back remaining bytes
                    receiveBuffer.SetLength(0);
                    receiveBuffer.Position = 0;
                    remaining.Position = 0;
                    await remaining.CopyToAsync(receiveBuffer, cancellationToken);
                    receiveBuffer.Position = 0;

                    return packet;
                }
            }
            catch (EndOfStreamException)
            {
                // Not enough data to deserialize full packet, continue reading
                receiveBuffer.Position = receiveBuffer.Length; // move to end for next write
                continue;
            }
            catch
            {
                // Unexpected error during deserialization, handle accordingly
                throw;
            }
        }
    }

    public async Task SendPacketAsync(Packet packet)
    {
        await WinterForge.SerializeToStringAsync(packet);
    }

    public abstract void Disconnect();
    public virtual void Dispose() => receiveBuffer?.Dispose();
}
