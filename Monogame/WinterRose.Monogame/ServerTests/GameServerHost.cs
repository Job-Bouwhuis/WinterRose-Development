using SharpDX.WIC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Networking.TCP;

namespace WinterRose.Monogame.Servers;

public class GameServerHost : ObjectComponent
{
    TCPServer server = new();

    public GameServerHost(string? ip, int host, bool makeConsole)
    {
        if (makeConsole)
        {
            Windows.OpenConsole();
        }
        server.OnMessageReceived += Server_OnMessageReceived;

        server.Start(ip ?? IPAddress.Any.ToString(), host);

        ExitHelper.GameClosing += GameClosing;
    }

    private void Server_OnMessageReceived(string message, TCPClientInfo sender, TCPClientInfo? relayClient)
    {
        if (message.StartsWith("Pos "))
            server.Send(message);
    }

    private void GameClosing() => server.Dispose();
}
