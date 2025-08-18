using System;
using System.Net;

namespace WinterRose.NetworkServer;
public class ClusterInvitation
{
    public string ClusterId { get; set; }
    public string Version { get; set; }
    public Guid NodeId { get; set; }
    public IPAddress NodeAddress { get; set; }
    public int TcpPort { get; set; }
}