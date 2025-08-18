using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeVein.Connections;
public abstract class NetworkConnection
{
    protected ILogger logger;

    public string? ClusterId { get; protected set; }
    public Guid NodeId { get; } = Guid.NewGuid();

    public NetworkConnection(ILogger logger, string clusterId)
    {
        this.logger = logger;
        ClusterId = clusterId;
    }

    public abstract Task StartAsync(CancellationToken token = default);
    public abstract Task StopAsync();

    public abstract Stream GetStream();
}

