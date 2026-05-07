using System.Net;

namespace WinterRose.ForgeVein.Networking.Transport;

public enum TransportProtocol
{
    Tcp,
    Udp
}

public readonly record struct TransportReceiveResult(ReadOnlyMemory<byte> Payload, EndPoint? RemoteEndPoint, DateTime Timestamp);

public interface ITransportConnection : IAsyncDisposable
{
    Guid ConnectionId { get; }
    TransportProtocol Protocol { get; }
    EndPoint? RemoteEndPoint { get; }
    ValueTask SendAsync(ReadOnlyMemory<byte> payload, CancellationToken cancellationToken = default);
    ValueTask<TransportReceiveResult> ReceiveAsync(CancellationToken cancellationToken = default);
    ValueTask CloseAsync(CancellationToken cancellationToken = default);
}

public interface ITransportListener : IAsyncDisposable
{
    TransportProtocol Protocol { get; }
    EndPoint EndPoint { get; }
    ValueTask StartAsync(CancellationToken cancellationToken = default);
    ValueTask StopAsync(CancellationToken cancellationToken = default);
    ValueTask<ITransportConnection> AcceptAsync(CancellationToken cancellationToken = default);
}
