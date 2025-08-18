using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeMantle.Values;

namespace WinterRose.ForgeVein.Configs;
public class ServerConfig : IConfigValue
{
    [WFInclude] public string ClusterId { get; set; } = "defaultCluster";
    [WFInclude] public int TcpPort { get; set; } = 53800;
    [WFInclude] public int UdpPort { get; set; } = 55620;

    public object? Get() => this;
    public Type ValueType => typeof(ServerConfig);
}

