using System.Net;

namespace WinterRose.ForgeVein.Networking.Transport;

/// <summary>
/// Factory for creating transport listeners and connections.
/// Provides convenient methods for creating TCP and UDP transports.
/// </summary>
public static class TransportFactory
{
    /// <summary>
    /// Creates a TCP transport listener on the specified endpoint.
    /// </summary>
    public static ITransportListener CreateTcpListener(IPEndPoint endpoint)
    {
        return new TcpTransportListener(endpoint);
    }

    /// <summary>
    /// Creates a TCP transport listener on a specific IP and port.
    /// </summary>
    public static ITransportListener CreateTcpListener(string address, int port)
    {
        var ipAddress = IPAddress.Parse(address);
        return CreateTcpListener(new IPEndPoint(ipAddress, port));
    }

    /// <summary>
    /// Creates a UDP transport listener on the specified endpoint.
    /// </summary>
    public static ITransportListener CreateUdpListener(IPEndPoint endpoint)
    {
        return new UdpTransportListener(endpoint);
    }

    /// <summary>
    /// Creates a UDP transport listener on a specific IP and port.
    /// </summary>
    public static ITransportListener CreateUdpListener(string address, int port)
    {
        var ipAddress = IPAddress.Parse(address);
        return CreateUdpListener(new IPEndPoint(ipAddress, port));
    }
}
