using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeVein.Configs;

namespace WinterRose.ForgeVein.Connections;
public class ServerConnection : Connection
{
    private readonly ServerConfig config;
    private TcpListener? listener;
    private CancellationTokenSource? cts;

    public int Port => config.TcpPort;

    public ServerConnection(ServerConfig config, ILogger logger)
        : base(logger, config.ClusterId)
    {
        this.config = config;
    }

    public override async Task StartAsync(CancellationToken token = default)
    {
        cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        listener = new TcpListener(IPAddress.Any, config.TcpPort);
        listener.Start();

        logger.LogInformation($"Server started on port {config.TcpPort} with cluster {ClusterId}");

        while (!cts.IsCancellationRequested)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();
            _ = HandleClientAsync(client, cts.Token);  // fire-and-forget client handler
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken token)
    {
        logger.LogInformation($"Client connected: {client.Client.RemoteEndPoint}");
        using var stream = client.GetStream();
        // TODO: packet processing loop
        await Task.Delay(-1, token);  // stub
    }

    public override Task StopAsync()
    {
        cts?.Cancel();
        listener?.Stop();
        return Task.CompletedTask;
    }

    public override Stream GetStream() => throw new NotSupportedException("ServerConnection manages multiple clients, use client streams.");
}

