using System.Net;

namespace WinterRose.NetworkServer.Packets;

public class ClientHelloPacket : Packet
{
    public class ClientHelloContent : PacketContent
    {
        public string ConnectedUsingString { get; set; }
        public IPAddress ConnectedUsingIPAddress { get; set; }
        public string HandshakeVersion { get; set; }
        public string AppVersion { get; set; }
        public string? Username { get; set; }

        public ClientHelloContent(string  connectedUsingString,
            IPAddress connectedUsingIPAddress,
            string handshakeVersion,
            string appVersion,
            string? username)
        {
            ConnectedUsingString = connectedUsingString;
            ConnectedUsingIPAddress = connectedUsingIPAddress;
            HandshakeVersion = handshakeVersion;
            AppVersion = appVersion;
            Username = username;
        }

        private ClientHelloContent() { } // for serialization
    }

    public ClientHelloPacket(string  connectedUsingString, IPAddress connectedUsingIPAddress, string handshakeVersion, string appVersion, string? username)
    {
        Header = new BasicHeader("ClientHello");
        Content = new ClientHelloContent(connectedUsingString, connectedUsingIPAddress, handshakeVersion, appVersion, username);
    }

    private ClientHelloPacket() { } // for serialization
}