using System;
using System.Net.Sockets;

namespace WinterRose.Networking.TCP;

public class TCPClientInfo
{
    [IncludeWithSerialization]
    public Guid Id { get; internal set; }
    public TcpClient Client { get; }
    [IncludeWithSerialization]
    public string Username { get; private set; } = null;

    public string MachineName => Client.Client.RemoteEndPoint.ToString();

    public bool ConnectionClosed { get; internal set; }

    public TCPClientInfo(TcpClient client)
    {
        Id = Guid.NewGuid();
        Client = client;
    }

    /// <summary>
    /// Exists for serialization
    /// </summary>
    private TCPClientInfo() { }

    public void SetUsername(string username, TCPUser user)
    {
        Username = username;

        user.SendAndResponseAsync($"^setusername^ {username}");
    }

    internal void SetName(string username) => Username = username;

    public void Close()
    {
        Client.Close();
        ConnectionClosed = true;
    }
}
