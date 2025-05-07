using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Networking;
using WinterRose.NetworkServer;

namespace WinterRoseServerService;
public class WinterRoseServer : BackgroundService
{
    private readonly ILogger<Worker> logger;
    private ServerConnection? server;

    public WinterRoseServer(ILogger<Worker> logger)
    {
        this.logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //IPAddress ip = Network.GetLocalIPAddress(); // however you're getting the IP
        IPAddress ip = IPAddress.Parse("192.168.2.15");
        int port = 12345;

        server = new ServerConnection(ip, port, logger);
        server.Start();
        logger.LogInformation("Server started on {ip}:{port}", ip, port);

        stoppingToken.Register(() =>
        {
            logger.LogInformation("Stopping server...");
            server.Disconnect();
            logger.LogInformation("Server stopped.");
        });

        // Keep this task alive until cancellation
        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Worker stopping");
        server?.Disconnect();
        return base.StopAsync(cancellationToken);
    }
}

