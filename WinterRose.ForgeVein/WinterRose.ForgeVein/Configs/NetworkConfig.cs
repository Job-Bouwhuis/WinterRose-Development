using WinterRose.ForgeMantle.Values;
namespace WinterRose.ForgeVein.Configs;

public class NetworkConfig : IConfigValue
{
    [WFInclude] public string ClusterId { get; set; } = "default";
    [WFInclude] public int TcpPort { get; set; } = 53800;
    [WFInclude] public int UdpPort { get; set; } = 55620;
    [WFInclude] public int HeartbeatInterval { get; set; } = 5000;

    public object? Get() => this;
    public Type ValueType => typeof(NetworkConfig);
}

